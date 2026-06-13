# APIBaiTap

Simple ASP.NET Core API for product management (exercise).

## Endpoints
- `POST /api/products` - Create a product (JSON body)
- `GET /api/products` - List all products
- `GET /api/products/{id}` - Get product by id

Validation for `id`:
- `id` must be a positive integer. If `id` is not positive, the API returns HTTP 400 with a JSON error array.

Example GET by id (invalid id):

```bash
curl -k https://localhost:5001/api/products/0
```

Response (400):

```json
[
  { "field": "id", "errors": ["Id must be a positive integer"] }
]
```

## Model
Request JSON shape:

{
  "name": "string (required, min 3 chars)",
  "price": number (required, > 0)
}

Validation rules:
- `name`: required, minimum 3 characters
- `price`: required, must be greater than 0

## Run locally
1. From the workspace root run:

```bash
dotnet run --project APIBaiTap/APIBaiTap.csproj
```

2. Note the HTTPS URL shown in the console (e.g. `https://localhost:5001`).

## Postman / HTTP example
Create a new request in Postman:

- Method: `POST`
- URL: `https://localhost:5001/api/products` (adjust port if different)
- Body -> raw -> JSON

Example valid body:

```json
{
  "name": "Phone",
  "price": 199.99
}
```

Successful response (201 Created):

```json
{
  "id": 1,
  "name": "Phone",
  "price": 199.99
}
```

Example invalid body (name too short, price <= 0):

```json
{
  "name": "PC",
  "price": 0
}
```

Validation failure response (HTTP 400): JSON array of field errors

```json
[
  { "field": "Name", "errors": ["Name must be at least 3 characters"] },
  { "field": "Price", "errors": ["Price must be greater than 0"] }
]
```

## Notes for submission
- Include this code, a short demo video showing the Postman request/response, and a brief README section describing how a front-end (web or mobile) can call the API (use the `POST /api/products` endpoint with JSON body and handle 201/400 responses).

If you want, I can create a small Postman collection file and a sample JavaScript fetch snippet for the frontend.
