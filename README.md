# MindAeon Dermalytics™

*AI-powered multimodal skin-biology intelligence platform integrating computer vision, proteomics,
systems biology, and explainable artificial intelligence for personalized skin-biology assessment.*

MindAeon Dermalytics is a research-grade, **non-diagnostic** platform for personalized skin-biology
reporting. It combines facial and skin images, quantitative protein biomarkers, demographics, and
lifestyle data into a structured biological report covering data quality control, domain and panel
selection, image scoring, protein normalization, integrated skin-quality scoring, phenotype
classification, and PDF export.

The public entry page (`/`) is a product landing page; authenticated users proceed to the application
through **Sign in**. Code namespaces, the database name, and configuration keys remain
`MindAeonDermaAI` for compatibility.

> **Disclaimer.** This software is intended for research use and cosmetic skin-biology interpretation
> only. It does not constitute a medical diagnosis and does not replace examination by a qualified
> dermatologist.

## License and Commercial Use

This project is released under the **MIT License**. Commercial use is expressly permitted: you may use,
copy, modify, merge, publish, distribute, sublicense, and sell copies of the software, subject to the
terms of the license. See [LICENSE](LICENSE) for the full text.

Organizations may deploy, customize, and integrate this platform into commercial products and services
without additional licensing fees. Attribution is appreciated but not required beyond the terms of the
MIT License.

## Developed with Mindaeon

This project was developed with **[Mindaeon](https://mindaeon.ai)**, an AI-assisted software engineering
environment. Mindaeon supported the architecture, implementation, and iterative refinement of the
platform. The resulting codebase is maintained as a standalone, production-oriented application.

## Stack

- ASP.NET Core 10 **MVC + Razor (.cshtml)**, EF Core 10, and **Microsoft SQL Server**
- ASP.NET Core Identity with roles **Admin / Clinician / Reviewer**
- Multimodal **LLM vision** image scoring through a provider abstraction (OpenAI-compatible and Anthropic)
- QuestPDF branded report export and Chart.js visualizations

## Solution Layout

```
src/MindAeonDermaAI.Web             MVC application (controllers, Razor views, static assets)
src/MindAeonDermaAI.Core            Entities, enums, DTOs, and service contracts
src/MindAeonDermaAI.Infrastructure  EF Core DbContext, migrations, seeders, and AI clients
src/MindAeonDermaAI.Analysis        Deterministic scoring engine and image analyzer
```

## Key Product Rules

1. **Database-verified data** — every categorical value is represented by a real database row, and
   scores are persisted to Microsoft SQL Server.
2. **Controlled categorical input** — categorical fields use the Smart-Select component (search plus
   inline creation), backed by `GET/POST /api/lookup/{entity}`.
3. **Configurable AI providers** — seeded templates support DeepSeek (default), OpenAI, Anthropic,
   Gemini, Azure OpenAI, and a Custom OpenAI-compatible endpoint. API keys are encrypted at rest using
   ASP.NET Data Protection and are managed under **Admin → AI Providers**.

## Configuration and Credential Handling

This repository contains **no credentials**. Connection strings, API keys, webhook secrets, and the
initial administrator password must be supplied at runtime through user secrets, environment
variables, or a server-side configuration file that is never committed.

- **Development:** store the connection string in user secrets:
  ```
  dotnet user-secrets set "ConnectionStrings:Default" "Server=<sql-host>,<port>;Database=<database>;User Id=<username>;Password=<strong-password>;TrustServerCertificate=True;Encrypt=True" --project src/MindAeonDermaAI.Web
  ```
- **Production (IIS):** provide the connection string through the `ConnectionStrings__Default`
  environment variable (system or application-pool level) or a server-only
  `appsettings.Production.json`. The file is excluded by `.gitignore`.
- **Public API:** set `Api__Key` to a strong, unique value before enabling the REST API. If it is empty,
  API requests with an `X-Api-Key` header are rejected.
- **Administrator account:** set `Seed__AdminEmail` and `Seed__AdminPassword` before the first run. If
  no password is provided, the seeder generates a one-time random password and prints it to the startup
  console. Change the password immediately after first sign-in.

Never commit `App_Data/`, Data Protection key rings, uploaded images, `appsettings.Production.json`, or
local environment files. These paths are excluded by `.gitignore`.

## Run Locally

```
dotnet run --project src/MindAeonDermaAI.Web
```

On startup the application applies EF Core migrations and seeds reference data (lookups, biological
domains, panels, biomarkers, phenotypes, and AI provider templates) together with the administrator
account.

Seed-only execution, useful for deployment verification:

```
dotnet run --project src/MindAeonDermaAI.Web -- --seed
```

## Deploy to Windows IIS

1. Install the **.NET 10 Hosting Bundle** on the target server (ASP.NET Core Module v2).
2. Publish the application:
   ```
   dotnet publish src/MindAeonDermaAI.Web -c Release -o C:\inetpub\mindaeondermaai
   ```
   The `web.config` required by the ASP.NET Core Module is generated automatically.
3. Create an IIS site with an HTTPS binding and a valid certificate.
4. Configure `ConnectionStrings__Default` for the application-pool identity, or place a server-only
   `appsettings.Production.json` on the server. Ensure the SQL Server endpoint is reachable.
5. Grant the application-pool identity write access to `App_Data\`. Data Protection keys are persisted
   under `App_Data\keys`, and uploaded images under `App_Data\uploads`.
6. Sign in and open **Admin → AI Providers** to configure provider keys and run **Test connection**.
   Without a provider key, the platform still produces deterministic scores and a fallback narrative.

## End-to-End Workflow

Subjects → new assessment (demographics and lifestyle through Smart-Select, image upload, protein
entry) → analyze images → run analysis → generate report → on-screen report and **PDF download**. A
fully synthetic, clearly labeled **Example Report** is available from the sidebar for demonstrations.

## Lisans ve Ticari Kullanım

Bu proje **MIT Lisansı** ile yayımlanmıştır. Ticari kullanım açıkça izin verilen kapsamdadır; yazılımı
kullanabilir, kopyalayabilir, değiştirebilir, birleştirebilir, yayımlayabilir, alt lisanslayabilir ve
satabilirsiniz. Koşullar için [LICENSE](LICENSE) dosyasına bakınız.

Kurumlar bu platformu ek lisans ücreti ödemeden ticari ürün ve hizmetlerine entegre edebilir,
özelleştirebilir ve dağıtabilir.

Bu proje **[Mindaeon](https://mindaeon.ai)** ile geliştirilmiştir. Mindaeon; mimari, uygulama ve iteratif
iyileştirme süreçlerinde kullanılmıştır. Kod tabanı, bağımsız ve üretim odaklı bir uygulama olarak
sürdürülmektedir.
