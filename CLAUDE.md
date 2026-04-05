# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Umbraco Impersonator is a package for Umbraco CMS (v17+) that allows administrators to impersonate other backoffice users. This is useful for checking permissions without knowing user passwords.

## Build Commands

### .NET Backend
```bash
# Build the solution
dotnet build src/Our.Umbraco.Impersonator/Our.Umbraco.Impersonator.sln

# Run the test site (for development)
dotnet run --project src/TestSite/TestSite.csproj

# Create NuGet package
dotnet pack src/Our.Umbraco.Impersonator/Our.Umbraco.Impersonator.csproj
```

### TypeScript/Frontend (Client)
```bash
cd src/Our.Umbraco.Impersonator/Client

# Install dependencies
npm install

# Build the frontend (outputs to wwwroot/)
npm run build

# Watch mode for development
npm run dev

# Regenerate API client from OpenAPI spec (requires TestSite running)
npm run generate-client
```

## Architecture

### Backend (.NET)
- **ImpersonatorComposer.cs**: Registers services, configures Swagger docs for the "impersonator" API, and sets up custom operation ID handling for cleaner generated TypeScript client methods.
- **UserController.cs**: Management API controller with endpoints for impersonation:
  - `GET /impersonator/api/v1/GetImpersonatingUserHash` - Check if currently impersonating
  - `POST /impersonator/api/v1/Impersonate?id={guid}` - Impersonate a user by GUID
  - `POST /impersonator/api/v1/EndImpersonation` - End impersonation and return to original user
- **ImpersonatedUserId.cs**: Model for tracking impersonation state in session

### Frontend (TypeScript/Lit)
Located in `src/Our.Umbraco.Impersonator/Client/`:
- **src/bundle.manifests.ts**: Entry point that combines all extension manifests
- **src/impersonator/**: Contains the main `impersonator-app.ts` Lit web component and manifest
- **src/api/**: Auto-generated TypeScript client from OpenAPI spec via `@hey-api/openapi-ts`
- **vite.config.ts**: Builds to `../wwwroot/umbraco-impersonator.js`

### Package Registration
- **wwwroot/umbraco-package.json**: Declares the package bundle for Umbraco's backoffice to load
- Static assets served from `App_Plugins/umbraco-impersonator/`

## Key Patterns

- The frontend uses Umbraco's Lit-based component system (`UmbElementMixin`)
- The extension type is `userProfileApp` - appears in the user profile dialog
- API client is generated from Swagger/OpenAPI spec at `https://localhost:44320/umbraco/swagger/impersonator/swagger.json`
- Authorization requires `SectionAccessContent` policy (users with access to Content section)

## Test Site

The `src/TestSite/` project is a minimal Umbraco installation for local development that references the package project directly.
