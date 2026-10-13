# Reprojection — baseline

ASP.NET Core MVC app that calls the vendor's policy API, maps the JSON to C# models, and displays it.
Purpose of this baseline: prove the data is coming in and landing in the right models.

## Run it

1. Open `Reprojection.sln` in Visual Studio 2026 and press F5.
2. Enter any policy number. Mock mode is on by default, so you'll get `MockData/policy-sample.json`.

Targets `net10.0`. If your org is on .NET 8/9, change `<TargetFramework>` in the .csproj — nothing else depends on 10.

## Switch to the real vendor API

1. In `appsettings.json` → `VendorApi`, set `BaseUrl`, the two `Endpoints`, and `Auth.Scheme` / `Auth.HeaderName`.
2. Put the key in User Secrets, not appsettings (right-click project → Manage User Secrets):
   ```json
   { "VendorApi": { "Auth": { "ApiKey": "..." } } }
   ```
   On servers use the env var `VendorApi__Auth__ApiKey`.
3. Set `"UseMockData": false` (in `appsettings.Development.json` too — it overrides).

## Fit the models to the real JSON

The models in `Models/Vendor/PolicyInquiryResponse.cs` are a starting shape, not the vendor's contract.

1. Drop a real response into `MockData/policy-sample.json` (or `policy-<number>.json` to keep several).
2. Load the page. The **Mapping check** panel lists:
   - **In JSON, not in models** — fields being dropped; add properties.
   - **In models, not in JSON** — wrong name, or genuinely optional.
3. If a field's type doesn't match (e.g. a string where a decimal is expected), the page shows the exact JSON path and line.
4. Shortcut for big payloads: copy the JSON, then **Edit → Paste Special → Paste JSON As Classes**.

## Layout

```
Configuration/VendorApiOptions.cs     strongly typed "VendorApi" settings, validated at startup
Services/VendorPolicyApiClient.cs     typed HttpClient: GET policy (API #1), POST reprojection (API #2), mock mode
Services/JsonMappingInspector.cs      raw JSON vs mapped model diff
Services/VendorJson.cs                shared serializer settings
Models/Vendor/                        vendor DTOs
Controllers/PolicyController.cs       /Policy?policyNumber=...
Views/Policy/Index.cshtml             the page
MockData/                             sample payloads
```

API #2 (`RunReprojectionAsync`) is configured and callable but not wired to the UI yet; its response models come once we have a real sample.
