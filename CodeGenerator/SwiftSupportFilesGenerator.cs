using System.IO;

namespace CodeGenerator
{
    /// <summary>
    /// Writes the Swift files that are the same for every database: the API client, the form helpers
    /// (parsing, validation, the text field row), the display formatting used by the list and detail screens, and ResultState.
    /// These don't depend on the tables, so this is not a Generator.
    /// </summary>
    public class SwiftSupportFilesGenerator
    {
        public void Generate(string destinationFolder)
        {
            WriteFile(destinationFolder + "APIClient.swift", ApiClientSource);
            WriteFile(destinationFolder + "FormSupport.swift", FormSupportSource);
            WriteFile(destinationFolder + "DisplayFormat.swift", DisplayFormatSource);
            WriteFile(destinationFolder + "ResultState.swift", ResultStateSource);
        }

        private static void WriteFile(string filePath, string contents)
        {
            Generator.EnsureDirectoryExists(filePath);
            File.WriteAllText(filePath, contents);
        }

        private const string ResultStateSource = @"enum ResultState<T> {
    case idle
    case loading
    case success(T)
    case failure(Error)
}
";

        private const string ApiClientSource = @"import Foundation

/// Where the API lives and how requests are authorised.
struct APIConfiguration {
    /// The root address of the server, e.g. https://example.com
    var baseURL: URL
    /// The path the MVC controllers are mounted under ([Route(""api/[controller]"")]).
    var apiPrefix = ""api""
    /// Optional hook that adds authentication (a bearer token, a request signature, ...) to every request.
    var authorize: ((inout URLRequest) async throws -> Void)? = nil
}

enum APIError: LocalizedError {
    case invalidResponse
    case emptyResponse
    case http(statusCode: Int, message: String?)

    var errorDescription: String? {
        switch self {
        case .invalidResponse:
            return ""The server returned a response that could not be understood.""
        case .emptyResponse:
            return ""The server returned no data.""
        case .http(let statusCode, let message):
            if statusCode == 404 {
                return ""The item could not be found (HTTP 404).""
            }
            if let message = message, !message.isEmpty {
                return ""The server returned an error (HTTP \(statusCode)): \(message)""
            }
            return ""The server returned an error (HTTP \(statusCode)).""
        }
    }
}

/// Talks JSON to the ASP.NET controllers. The generated repositories are built on this.
final class APIClient {
    private let configuration: APIConfiguration
    private let session: URLSession
    private let decoder: JSONDecoder
    private let encoder: JSONEncoder

    init(configuration: APIConfiguration, session: URLSession = .shared) {
        self.configuration = configuration
        self.session = session

        let decoder = JSONDecoder()
        decoder.dateDecodingStrategy = .custom(APIClient.decodeDate)
        self.decoder = decoder

        let encoder = JSONEncoder()
        encoder.dateEncodingStrategy = .iso8601
        self.encoder = encoder
    }

    func get<Response: Decodable>(_ resource: String, id: String? = nil) async throws -> Response {
        let request = makeRequest(method: ""GET"", resource: resource, id: id)
        return try await send(request)
    }

    func post<Body: Encodable, Response: Decodable>(_ resource: String, body: Body) async throws -> Response {
        var request = makeRequest(method: ""POST"", resource: resource, id: nil)
        request.httpBody = try encoder.encode(body)
        return try await send(request)
    }

    func put<Body: Encodable>(_ resource: String, id: String, body: Body) async throws {
        var request = makeRequest(method: ""PUT"", resource: resource, id: id)
        request.httpBody = try encoder.encode(body)
        _ = try await perform(request)
    }

    func delete(_ resource: String, id: String) async throws {
        let request = makeRequest(method: ""DELETE"", resource: resource, id: id)
        _ = try await perform(request)
    }

    // MARK: - Requests

    private func makeRequest(method: String, resource: String, id: String?) -> URLRequest {
        var url = configuration.baseURL
        if !configuration.apiPrefix.isEmpty {
            url.appendPathComponent(configuration.apiPrefix)
        }
        url.appendPathComponent(resource)
        if let id = id {
            url.appendPathComponent(id)
        }

        var request = URLRequest(url: url)
        request.httpMethod = method
        request.setValue(""application/json"", forHTTPHeaderField: ""Accept"")
        if method == ""POST"" || method == ""PUT"" {
            request.setValue(""application/json"", forHTTPHeaderField: ""Content-Type"")
        }
        return request
    }

    private func perform(_ request: URLRequest) async throws -> Data {
        var request = request
        if let authorize = configuration.authorize {
            try await authorize(&request)
        }

        let (data, response) = try await session.data(for: request)
        guard let http = response as? HTTPURLResponse else {
            throw APIError.invalidResponse
        }
        guard (200..<300).contains(http.statusCode) else {
            let message = String(data: data, encoding: .utf8).map { String($0.prefix(200)) }
            throw APIError.http(statusCode: http.statusCode, message: message)
        }
        return data
    }

    private func send<Response: Decodable>(_ request: URLRequest) async throws -> Response {
        let data = try await perform(request)
        guard !data.isEmpty else {
            throw APIError.emptyResponse
        }

        do {
            return try decoder.decode(Response.self, from: data)
        } catch {
            // ASP.NET returns a bare, unquoted body when an action returns a string.
            if Response.self == String.self,
               let text = String(data: data, encoding: .utf8),
               let value = text.trimmingCharacters(in: CharacterSet(charactersIn: ""\"""")) as? Response {
                return value
            }
            throw error
        }
    }

    // MARK: - Dates

    // .NET writes dates like 2024-05-01T10:15:30, 2024-05-01T10:15:30.5 or 2024-05-01T10:15:30.1234567Z.
    // Values without an offset are read as UTC; dates are sent as UTC too. If your server stores local
    // times, change the formatter time zone below.
    private static let dateFormats = [
        ""yyyy-MM-dd'T'HH:mm:ss.SSSXXXXX"",
        ""yyyy-MM-dd'T'HH:mm:ssXXXXX"",
        ""yyyy-MM-dd'T'HH:mm:ss.SSS"",
        ""yyyy-MM-dd'T'HH:mm:ss"",
        ""yyyy-MM-dd""
    ]

    private static let dateFormatters: [DateFormatter] = dateFormats.map { format in
        let formatter = DateFormatter()
        formatter.locale = Locale(identifier: ""en_US_POSIX"")
        formatter.timeZone = TimeZone(secondsFromGMT: 0)
        formatter.dateFormat = format
        return formatter
    }

    private static func decodeDate(from decoder: Decoder) throws -> Date {
        let container = try decoder.singleValueContainer()
        let raw = try container.decode(String.self)
        let text = normalisedFractionalSeconds(raw)

        for formatter in dateFormatters {
            if let date = formatter.date(from: text) {
                return date
            }
        }
        throw DecodingError.dataCorruptedError(in: container, debugDescription: ""Unrecognised date: \(raw)"")
    }

    /// Pads or trims the fractional seconds to exactly three digits so one format can read them all.
    private static func normalisedFractionalSeconds(_ text: String) -> String {
        guard let dot = text.firstIndex(of: ""."") else {
            return text
        }

        var end = text.index(after: dot)
        while end < text.endIndex, (""0""...""9"").contains(text[end]) {
            end = text.index(after: end)
        }

        let digits = String(text[text.index(after: dot)..<end])
        let threeDigits = String((digits + ""000"").prefix(3))
        return String(text[..<dot]) + ""."" + threeDigits + String(text[end...])
    }
}
";

        private const string FormSupportSource = @"import SwiftUI

/// The state of a save operation.
enum SaveState: Equatable {
    case idle
    case saving
    case saved
    case failed(String)
}

/// Reads and writes the numbers typed into form fields, using the user's locale.
enum FormParsing {
    static func isBlank(_ text: String) -> Bool {
        text.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
    }

    static func nilIfBlank(_ text: String) -> String? {
        isBlank(text) ? nil : text
    }

    static func int32(_ text: String) -> Int32? {
        guard let value = number(from: text), value == value.rounded(),
              value >= Double(Int32.min), value <= Double(Int32.max) else {
            return nil
        }
        return Int32(value)
    }

    static func double(_ text: String) -> Double? {
        number(from: text)
    }

    static func float(_ text: String) -> Float? {
        number(from: text).map { Float($0) }
    }

    static func string(from value: Int32) -> String {
        String(value)
    }

    static func string(from value: Double) -> String {
        formatter.string(from: NSNumber(value: value)) ?? String(value)
    }

    static func string(from value: Float) -> String {
        // Going through the shortest text form avoids showing 0.1 as 0.10000000149.
        string(from: Double(String(value)) ?? Double(value))
    }

    private static let formatter: NumberFormatter = {
        let formatter = NumberFormatter()
        formatter.numberStyle = .decimal
        formatter.locale = .current
        formatter.usesGroupingSeparator = false
        formatter.maximumFractionDigits = 10
        return formatter
    }()

    private static func number(from text: String) -> Double? {
        let trimmed = text.trimmingCharacters(in: .whitespacesAndNewlines)
        guard !trimmed.isEmpty else {
            return nil
        }
        return formatter.number(from: trimmed)?.doubleValue
    }
}

/// Validation rules shared by the generated form fields. Each returns a message, or nil when the value is valid.
enum FormValidation {
    static func text(_ value: String, label: String, required: Bool, maxLength: Int? = nil) -> String? {
        if FormParsing.isBlank(value) {
            return required ? ""\(label) is required."" : nil
        }
        if let maxLength = maxLength, value.count > maxLength {
            return ""\(label) must be \(maxLength) characters or fewer.""
        }
        return nil
    }

    static func guid(_ value: String, label: String, required: Bool) -> String? {
        if FormParsing.isBlank(value) {
            return required ? ""\(label) is required."" : nil
        }
        return UUID(uuidString: value) == nil ? ""\(label) must be a valid GUID."" : nil
    }

    static func integer(_ value: String, label: String, required: Bool) -> String? {
        if FormParsing.isBlank(value) {
            return required ? ""\(label) is required."" : nil
        }
        return FormParsing.int32(value) == nil ? ""\(label) must be a whole number."" : nil
    }

    static func decimal(_ value: String, label: String, required: Bool) -> String? {
        if FormParsing.isBlank(value) {
            return required ? ""\(label) is required."" : nil
        }
        return FormParsing.double(value) == nil ? ""\(label) must be a number."" : nil
    }
}

/// A text field with its validation message underneath.
struct FormTextField: View {
    let title: String
    @Binding var text: String
    var error: String? = nil
    var keyboard: UIKeyboardType = .default
    var multiline: Bool = false

    var body: some View {
        VStack(alignment: .leading, spacing: 4) {
            if multiline {
                TextField(title, text: $text, axis: .vertical)
                    .lineLimit(3...8)
            } else {
                TextField(title, text: $text)
                    .keyboardType(keyboard)
            }

            if let error = error {
                Text(error)
                    .font(.caption)
                    .foregroundStyle(.red)
            }
        }
    }
}
";

        private const string DisplayFormatSource = @"import Foundation

/// Turns column values into text for the list and detail screens.
enum DisplayFormat {
    static func text(_ value: String?) -> String {
        value ?? ""-""
    }

    static func text(_ value: Int32?) -> String {
        value.map { String($0) } ?? ""-""
    }

    static func text(_ value: Float?) -> String {
        value.map { $0.formatted() } ?? ""-""
    }

    static func text(_ value: Double?) -> String {
        value.map { $0.formatted() } ?? ""-""
    }

    static func text(_ value: Bool?) -> String {
        value.map { $0 ? ""Yes"" : ""No"" } ?? ""-""
    }

    static func dateText(_ value: Date?, includeTime: Bool) -> String {
        guard let value = value else {
            return ""-""
        }
        return value.formatted(date: .abbreviated, time: includeTime ? .shortened : .omitted)
    }
}
";
    }
}
