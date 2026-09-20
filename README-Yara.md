# Yara's Backend Work – Riwaq

This document tracks the backend features implemented by **Yara Kmail** in the Riwaq project.

It will be updated as new features are completed, including:

- What was implemented
- API endpoints
- Important business rules
- Related code files
- Verification and testing results

---

## Project

**Riwaq – Educational Knowledge & Experience Sharing Platform**

### Backend Technologies

- ASP.NET Core Web API
- C#
- Entity Framework Core
- PostgreSQL
- Firebase Authentication

The implementation follows the project's existing feature-based structure and the API contract defined in the Technical Design Document.

---

# Completed Features

## 1. Educational Content

### What was implemented

Implemented the **Educational Content** feature according to the project's Technical Design Document.

The feature supports:

- Creating educational content
- Retrieving all educational content
- Retrieving a specific educational content item
- Updating educational content
- Deleting educational content
- Authentication context for the current user
- Ownership checks for update and delete operations

### Content Data

Each educational content item supports:

| Field         | Description                        |
| ------------- | ---------------------------------- |
| `title`       | Title of the educational content   |
| `description` | Description of the content         |
| `contentType` | Type of educational content        |
| `contentUrl`  | URL of the content when applicable |

Supported content types:

- `Text`
- `Image`
- `Video`
- `File`
- `ExternalLink`

### API Endpoints

| Method | Endpoint                        | Purpose                     |
| ------ | ------------------------------- | --------------------------- |
| GET    | `/api/educational-content`      | Get all educational content |
| GET    | `/api/educational-content/{id}` | Get a specific content item |
| POST   | `/api/educational-content`      | Create educational content  |
| PATCH  | `/api/educational-content/{id}` | Update educational content  |
| DELETE | `/api/educational-content/{id}` | Delete educational content  |

### Code

- [EducationalContentController](Team3.Backend/Features/EducationalContent/EducationalContentController.cs)
- [EducationalContentService](Team3.Backend/Features/EducationalContent/EducationalContentService.cs)
- [EducationalContentRepository](Team3.Backend/Features/EducationalContent/EducationalContentRepository.cs)
- [IEducationalContentService](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentService.cs)
- [IEducationalContentRepository](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentRepository.cs)

### DTOs

- [CreateEducationalContentRequest](Team3.Backend/Features/EducationalContent/Dtos/CreateEducationalContentRequest.cs)
- [UpdateEducationalContentRequest](Team3.Backend/Features/EducationalContent/Dtos/UpdateEducationalContentRequest.cs)
- [EducationalContentResponse](Team3.Backend/Features/EducationalContent/Dtos/EducationalContentResponse.cs)

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

- Has no request body
- Creates a new Share record
- Allows multiple share events
- Returns `204 No Content`
- Does not have a DELETE endpoint

### Code### Code

- [EducationalContentController](Team3.Backend/Features/EducationalContent/EducationalContentController.cs)
- [EducationalContentInteractionsService](Team3.Backend/Features/EducationalContent/EducationalContentInteractionsService.cs)
- [EducationalContentInteractionsRepository](Team3.Backend/Features/EducationalContent/EducationalContentInteractionsRepository.cs)
- [IEducationalContentInteractionsService](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentInteractionsService.cs)
- [IEducationalContentInteractionsRepository](Team3.Backend/Features/EducationalContent/Interfaces/IEducationalContentInteractionsRepository.cs)

### Data Models

- [EducationalContent](Team3.Backend/Models/EducationalContent.cs)
- [Share](Team3.Backend/Models/Share.cs)

The `Share` model contains:

- `Id`
- `UserId`
- `EducationalContentId`
- `CreatedAt`

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

- HTTP routing
- Reading the current authentication context
- Returning appropriate HTTP status codes
- Passing requests to the service layer

### Service

Responsible for business logic such as:

- Validating IDs
- Finding the local user from the Firebase UID
- Checking that educational content exists
- Checking ownership for protected operations
- Preventing duplicate Like/Save/Repost states
- Creating Share events

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

---

## 3. Comments

The Comments feature is implemented as a separate backend feature module under the Post Interaction & Engagement functionality.

### What was implemented

The feature supports:

- Creating comments on educational content
- Retrieving comments for educational content
- Updating comments
- Deleting comments
- Replying to comments using `parentCommentId`
- Ownership checks for update and delete operations
- Educational content existence validation
- Parent comment validation to ensure replies belong to the same educational content

### API Endpoints

| Method | Endpoint                                        | Purpose                              |
| ------ | ----------------------------------------------- | ------------------------------------ |
| GET    | `/api/educational-content/{contentId}/comments` | Get comments for educational content |
| POST   | `/api/educational-content/{contentId}/comments` | Create a comment                     |
| PATCH  | `/api/comments/{id}`                            | Update a comment                     |
| DELETE | `/api/comments/{id}`                            | Delete a comment                     |

### Request Data

Creating a comment supports:

| Field             | Description                                             |
| ----------------- | ------------------------------------------------------- |
| `content`         | Comment text                                            |
| `parentCommentId` | Optional ID of the parent comment when creating a reply |

Updating a comment supports:

| Field     | Description          |
| --------- | -------------------- |
| `content` | Updated comment text |

### Business Rules

- A comment must belong to an existing educational content item.
- Comment content cannot be empty.
- A reply must reference a comment belonging to the same educational content.
- Only the comment owner can update or delete the comment.
- Replies use the same Comment resource through `parentCommentId`; a separate Reply resource is not required.
- The authenticated user's local `UserId` is resolved from the Firebase UID.

### Code

- [CommentsController](Team3.Backend/Features/Comments/CommentsController.cs)
- [CommentsService](Team3.Backend/Features/Comments/CommentsService.cs)
- [CommentsRepository](Team3.Backend/Features/Comments/CommentsRepository.cs)
- [ICommentsService](Team3.Backend/Features/Comments/Interfaces/ICommentsService.cs)
- [ICommentsRepository](Team3.Backend/Features/Comments/Interfaces/ICommentsRepository.cs)

### DTOs

- [CommentResponse](Team3.Backend/Features/Comments/DTOs/CommentResponse.cs)
- [CreateCommentRequest](Team3.Backend/Features/Comments/DTOs/CreateCommentRequest.cs)
- [UpdateCommentRequest](Team3.Backend/Features/Comments/DTOs/UpdateCommentRequest.cs)

### Data Model

- [Comment](Team3.Backend/Models/Comment.cs)

The `Comment` model contains:

- `Id`
- `UserId`
- `EducationalContentId`
- `ParentCommentId`
- `Content`
- `CreatedAt`
- `UpdatedAt`

The `ParentCommentId` relationship supports nested replies while using the same Comment entity.

### Verification

The Comments implementation was included in the successful project build.

## 4. Connection Requests

The Connection Requests feature allows users to send connection requests, view sent and received requests, and manage the request status.

### What was implemented

The feature supports:

- Sending connection requests
- Retrieving received connection requests
- Retrieving sent connection requests
- Accepting connection requests
- Rejecting connection requests
- Cancelling pending connection requests
- Preventing connection requests to the current user
- Preventing duplicate pending requests
- Preventing connection requests between already connected users
- Creating a Connection automatically when a request is accepted
- Authorization checks so only the sender or receiver can update a request
- Validating supported request statuses

### API Endpoints

| Method | Endpoint                            | Purpose                              |
| ------ | ----------------------------------- | ------------------------------------ |
| POST   | `/api/connection-requests`          | Send a connection request            |
| GET    | `/api/connection-requests/received` | Get received connection requests     |
| GET    | `/api/connection-requests/sent`     | Get sent connection requests         |
| PATCH  | `/api/connection-requests/{id}`     | Update the connection request status |

### Request Data

Sending a connection request supports:

| Field            | Description                                            |
| ---------------- | ------------------------------------------------------ |
| `receiverUserId` | ID of the user who will receive the connection request |

Updating a connection request supports:

| Field    | Description        |
| -------- | ------------------ |
| `status` | New request status |

Supported statuses:

- `Pending`
- `Accepted`
- `Rejected`
- `Cancelled`

### Business Rules

- A user cannot send a connection request to themselves.
- A connection request cannot be created if the users are already connected.
- A duplicate pending request between the same sender and receiver is not allowed.
- Only the sender can cancel a pending request.
- Only the receiver can accept or reject a pending request.
- A connection is created automatically when a request is accepted.
- Only the sender or receiver can update the connection request.
- The authenticated user's local `UserId` is resolved from the Firebase UID.
- The Firebase UID is obtained from the `X-Firebase-Uid` request header.

### Code

- [ConnectionRequestsController](Team3.Backend/Features/ConnectionRequests/ConnectionRequestsController.cs)
- [ConnectionRequestsService](Team3.Backend/Features/ConnectionRequests/ConnectionRequestsService.cs)
- [ConnectionRequestsRepository](Team3.Backend/Features/ConnectionRequests/ConnectionRequestsRepository.cs)
- [IConnectionRequestsService](Team3.Backend/Features/ConnectionRequests/Interfaces/IConnectionRequestsService.cs)
- [IConnectionRequestsRepository](Team3.Backend/Features/ConnectionRequests/Interfaces/IConnectionRequestsRepository.cs)

### DTOs

- [ConnectionRequestResponse](Team3.Backend/Features/ConnectionRequests/DTOs/ConnectionRequestResponse.cs)
- [SendConnectionRequestRequest](Team3.Backend/Features/ConnectionRequests/DTOs/SendConnectionRequestRequest.cs)
- [UpdateConnectionRequestStatusRequest](Team3.Backend/Features/ConnectionRequests/DTOs/UpdateConnectionRequestStatusRequest.cs)

### Data Model

- [ConnectionRequest](Team3.Backend/Models/ConnectionRequest.cs)
- [Connection](Team3.Backend/Models/Connection.cs)

The `ConnectionRequest` model contains:

- `Id`
- `SenderUserId`
- `ReceiverUserId`
- `Status`
- `CreatedAt`
- `UpdatedAt`

The `Connection` model is created when a connection request is accepted.

### Verification

The Connection Requests implementation was successfully compiled using:

```powershell
dotnet build .\Team3.Backend\Team3.Backend.csproj
```

Result:

```text
Build succeeded.
```

## 5. Connections

The Connections feature allows authenticated users to view and manage their established connections.

### What was implemented

The feature supports:

- Retrieving the authenticated user's connections
- Retrieving a specific connection by ID
- Deleting an existing connection
- Authorization checks so users can only access or delete their own connections
- Preventing duplicate connections between the same two users
- Preventing a user from being connected to themselves
- Automatically creating a Connection when a connection request is accepted

### API Endpoints

| Method | Endpoint                | Purpose                                  |
| ------ | ----------------------- | ---------------------------------------- |
| GET    | `/api/connections`      | Get the authenticated user's connections |
| GET    | `/api/connections/{id}` | Get a specific connection                |
| DELETE | `/api/connections/{id}` | Delete an existing connection            |

### Business Rules

- A user can only retrieve their own connections.
- A user can only retrieve a connection if they are one of its participants.
- A user can only delete a connection they are part of.
- A connection cannot exist between a user and themselves.
- Duplicate connections between the same two users are prevented at the database level.
- Connection deletion is restricted to the users involved in the connection.
- The authenticated user's local `UserId` is resolved from the current user service.

### Code

- [ConnectionsController](Team3.Backend/Features/Connections/ConnectionsController.cs)
- [ConnectionsService](Team3.Backend/Features/Connections/ConnectionsService.cs)
- [ConnectionsRepository](Team3.Backend/Features/Connections/ConnectionsRepository.cs)
- [IConnectionsService](Team3.Backend/Features/Connections/Interfaces/IConnectionsService.cs)
- [IConnectionsRepository](Team3.Backend/Features/Connections/Interfaces/IConnectionsRepository.cs)

### DTOs

- [ConnectionResponse](Team3.Backend/Features/Connections/Dtos/ConnectionResponse.cs)

### Data Model

- [Connection](Team3.Backend/Models/Connection.cs)

The `Connection` entity represents an established connection between two users using:

- `UserAId`
- `UserBId`

A unique database index was added on `(UserAId, UserBId)` to prevent duplicate connections.

### Database Migration

A migration was added to enforce the duplicate-connection constraint:

- `20260919205153_PreventDuplicateConnections`

### Verification

The Connections feature was successfully compiled and included in the project test suite.

```powershell
dotnet build .\Riwaq.Backend.sln
dotnet test .\Riwaq.Backend.sln
```

Result:

```text
Build succeeded.
76 tests passed.
```
