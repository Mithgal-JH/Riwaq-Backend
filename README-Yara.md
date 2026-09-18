# Yara's Backend Work – Riwaq

This document tracks the backend features implemented by **Yara Kmail** in the Riwaq project.

It will be updated as new features are completed, including:

* What was implemented
* API endpoints
* Important business rules
* Related code files
* Verification and testing results

---

## Project

**Riwaq – Educational Knowledge & Experience Sharing Platform**

### Backend Technologies

* ASP.NET Core Web API
* C#
* Entity Framework Core
* PostgreSQL
* Firebase Authentication

The implementation follows the project's existing feature-based structure and the API contract defined in the Technical Design Document.

---

# Completed Features

## 1. Educational Content

### What was implemented

Implemented the **Educational Content** feature according to the project's Technical Design Document.

The feature supports:

* Creating educational content
* Retrieving all educational content
* Retrieving a specific educational content item
* Updating educational content
* Deleting educational content
* Authentication context for the current user
* Ownership checks for update and delete operations

### Content Data

Each educational content item supports:

| Field         | Description                        |
| ------------- | ---------------------------------- |
| `title`       | Title of the educational content   |
| `description` | Description of the content         |
| `contentType` | Type of educational content        |
| `contentUrl`  | URL of the content when applicable |

Supported content types:

* `Text`
* `Image`
* `Video`
* `File`
* `ExternalLink`

### API Endpoints

| Method | Endpoint                        | Purpose                     |
| ------ | ------------------------------- | --------------------------- |
| GET    | `/api/educational-content`      | Get all educational content |
| GET    | `/api/educational-content/{id}` | Get a specific content item |
| POST   | `/api/educational-content`      | Create educational content  |
| PATCH  | `/api/educational-content/{id}` | Update educational content  |
| DELETE | `/api/educational-content/{id}` | Delete educational content  |

### Code

* [EducationalContentController](Team3.Backend/Features/EducationalContent/EducationalContentController.cs)
* [EducationalContentService](Team3.Backend/Features/EducationalContent/EducationalContentService.cs)
* [EducationalContentRepository](Team3.Backend/Features/EducationalContent/EducationalContentRepository.cs)
* [IEducationalContentService](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentService.cs)
* [IEducationalContentRepository](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentRepository.cs)

### DTOs

* [CreateEducationalContentRequest](Team3.Backend/Features/EducationalContent/Dtos/CreateEducationalContentRequest.cs)
* [UpdateEducationalContentRequest](Team3.Backend/Features/EducationalContent/Dtos/UpdateEducationalContentRequest.cs)
* [EducationalContentResponse](Team3.Backend/Features/EducationalContent/Dtos/EducationalContentResponse.cs)

---

## 2. Educational Content Interactions

Educational Content interactions are implemented as operations under the Educational Content resource, following the Technical Design Document.

### Like

A user can like educational content and remove their like.

| Method | Endpoint                              | Purpose                |
| ------ | ------------------------------------- | ---------------------- |
| POST   | `/api/educational-content/{id}/likes` | Add a like             |
| DELETE | `/api/educational-content/{id}/likes` | Remove the user's like |

A duplicate like is prevented by checking whether the authenticated user has already liked the content.

### Save

A user can save educational content and remove their save.

| Method | Endpoint                              | Purpose                |
| ------ | ------------------------------------- | ---------------------- |
| POST   | `/api/educational-content/{id}/saves` | Save content           |
| DELETE | `/api/educational-content/{id}/saves` | Remove the user's save |

A duplicate save is prevented by checking whether the authenticated user has already saved the content.

### Repost

A user can repost educational content and remove their repost.

| Method | Endpoint                                | Purpose                  |
| ------ | --------------------------------------- | ------------------------ |
| POST   | `/api/educational-content/{id}/reposts` | Repost content           |
| DELETE | `/api/educational-content/{id}/reposts` | Remove the user's repost |

A duplicate repost is prevented by checking whether the authenticated user has already reposted the content.

### Share

A Share represents a **sharing event**, rather than a persistent user-controlled state.

| Method | Endpoint                               | Purpose                |
| ------ | -------------------------------------- | ---------------------- |
| POST   | `/api/educational-content/{id}/shares` | Record a sharing event |

The Share endpoint:

* Has no request body
* Creates a new Share record
* Allows multiple share events
* Returns `204 No Content`
* Does not have a DELETE endpoint

### Code### Code

- [EducationalContentController](Team3.Backend/Features/EducationalContent/EducationalContentController.cs)
- [EducationalContentInteractionsService](Team3.Backend/Features/EducationalContent/EducationalContentInteractionsService.cs)
- [EducationalContentInteractionsRepository](Team3.Backend/Features/EducationalContent/EducationalContentInteractionsRepository.cs)
- [IEducationalContentInteractionsService](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentInteractionsService.cs)
- [IEducationalContentInteractionsRepository](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentInteractionsRepository.cs)

### Data Models

* [EducationalContent](Team3.Backend/Models/EducationalContent.cs)
* [Share](Team3.Backend/Models/Share.cs)

The `Share` model contains:

* `Id`
* `UserId`
* `EducationalContentId`
* `CreatedAt`

Share records do not use a unique `(UserId, EducationalContentId)` constraint because the same user may share the same content multiple times.

---

# Architecture

The Educational Content feature follows the project's existing separation of responsibilities:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
EF Core / AppDbContext
    ↓
PostgreSQL
```

### Controller

Responsible for:

* HTTP routing
* Reading the current authentication context
* Returning appropriate HTTP status codes
* Passing requests to the service layer

### Service

Responsible for business logic such as:

* Validating IDs
* Finding the local user from the Firebase UID
* Checking that educational content exists
* Checking ownership for protected operations
* Preventing duplicate Like/Save/Repost states
* Creating Share events

### Repository

Responsible for database operations through Entity Framework Core.

---

# Authentication Context

The project uses Firebase Authentication for user authentication.

For the current local implementation, the Educational Content endpoints obtain the Firebase UID from the request's `X-Firebase-Uid` header and use it to locate the corresponding local `User` record.

The local `UserId` is not accepted from the client as part of the request body for these operations.

---

# Verification

The Educational Content implementation was successfully compiled using:

```powershell
dotnet build .\Team3.Backend\Team3.Backend.csproj
```

Result:

```text
Build succeeded.
```

