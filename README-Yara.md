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

### Code

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
182 tests passed.
```

## 6. Conversations & Messages

The Conversations & Messages feature provides email-style conversations between users who have an established connection.

### What was implemented

The feature supports:

- Retrieving the authenticated user's conversations
- Retrieving a specific conversation
- Retrieving messages within a conversation
- Sending messages
- Creating a conversation when the first message is sent
- Updating messages
- Authorization checks for conversation access
- Ownership checks for message updates
- Tracking conversation activity through `LastActivityAt`
- Supporting a subject for the first conversation message

### API Endpoints

| Method | Endpoint                                       | Purpose                                    |
| ------ | ---------------------------------------------- | ------------------------------------------ |
| GET    | `/api/conversations`                           | Get the authenticated user's conversations |
| GET    | `/api/conversations/{id}`                      | Get a specific conversation                |
| GET    | `/api/conversations/{conversationId}/messages` | Get messages in a conversation             |
| POST   | `/api/conversations/{conversationId}/messages` | Send a message                             |
| PATCH  | `/api/messages/{id}`                           | Update a message                           |

### Request Data

Sending a message supports:

| Field          | Description                                                                             |
| -------------- | --------------------------------------------------------------------------------------- |
| `connectionId` | Optional when the conversation already exists; required when creating the first message |
| `content`      | Message content                                                                         |
| `subject`      | Conversation subject; required when creating the first message                          |

Updating a message supports:

| Field     | Description             |
| --------- | ----------------------- |
| `content` | Updated message content |

### Response Data

A conversation response contains:

- `id`
- `subject`
- `participants`
- `lastActivityAt`

A message response contains:

- `id`
- `sender`
- `content`
- `createdAt`
- `updatedAt`

### Business Rules

- Only participants of the associated connection can access the conversation.
- A conversation is associated with a connection.
- A conversation can be created when the first message is sent.
- `connectionId` is required when creating the first conversation message.
- The authenticated user must belong to the specified connection.
- A connection cannot have more than one conversation.
- `subject` is required when creating the first conversation message.
- Message content cannot be empty.
- The authenticated user is automatically recorded as the message sender.
- Only the message owner can update a message.
- `LastActivityAt` is updated when a message is sent or updated.
- The authenticated user's local `UserId` is used to enforce authorization.

### Code

- [ConversationsController](Team3.Backend/Features/Conversations/ConversationsController.cs)
- [ConversationsService](Team3.Backend/Features/Conversations/ConversationsService.cs)
- [ConversationsRepository](Team3.Backend/Features/Conversations/ConversationsRepository.cs)
- [IConversationsService](Team3.Backend/Features/Conversations/Interfaces/IConversationsService.cs)
- [IConversationsRepository](Team3.Backend/Features/Conversations/Interfaces/IConversationsRepository.cs)

### DTOs

- [ConversationResponse](Team3.Backend/Features/Conversations/Dtos/ConversationResponse.cs)
- [MessageResponse](Team3.Backend/Features/Conversations/Dtos/MessageResponse.cs)
- [SendMessageRequest](Team3.Backend/Features/Conversations/Dtos/SendMessageRequest.cs)
- [UpdateMessageRequest](Team3.Backend/Features/Conversations/Dtos/UpdateMessageRequest.cs)

### Data Models

- [Conversation](Team3.Backend/Models/Conversation.cs)
- [Message](Team3.Backend/Models/Message.cs)

The `Conversation` entity stores the conversation subject and activity information and is linked to a `Connection`.

The `Message` entity stores the sender, content, timestamps, and associated conversation.

## 7. Ratings

The Ratings feature allows users to rate each other after completing a qualifying learning session.

### What was implemented

The feature supports:

- Submitting a rating for another participant after a completed learning session
- Scores from `1` to `5`
- Optional written reviews
- Preventing users from rating themselves
- Preventing duplicate ratings by the same user for the same learning session
- Identifying the other participant automatically as the rated user
- Retrieving ratings received by a user
- Authorization checks to ensure only learning session participants can submit ratings
- Immutable ratings with no update endpoint

### API Endpoints

| Method | Endpoint                                     | Purpose                                           |
| ------ | -------------------------------------------- | ------------------------------------------------- |
| POST   | `/api/learning-sessions/{sessionId}/ratings` | Submit a rating for the other session participant |
| GET    | `/api/profiles/{userId}/ratings`             | Get ratings received by a user                    |

### Request Data

Submitting a rating supports:

| Field    | Description                  |
| -------- | ---------------------------- |
| `score`  | Rating score from `1` to `5` |
| `review` | Optional written review      |

Example request:

```json
{
  "score": 5,
  "review": "Very helpful session."
}
```

### Response Data

A rating response contains:

- `id`
- `score`
- `review`
- `rater`
- `ratedUser`
- `learningSessionId`
- `createdAt`

### Business Rules

- Only participants of the associated learning session can submit a rating.
- Ratings can only be submitted after the learning session is `Completed`.
- A user cannot rate themselves.
- Each participant can submit only one rating for the other participant in the same learning session.
- The rated user is determined automatically from the session participants.
- The authenticated user is recorded as the rater.
- The score must be between `1` and `5`.
- The review is optional.
- Ratings cannot be edited after submission.
- Ratings are linked to the actual learning session.
- Ratings are retrieved by the user receiving them.

### Code

- [RatingsController](Team3.Backend/Features/Ratings/RatingsController.cs)
- [RatingsService](Team3.Backend/Features/Ratings/RatingsService.cs)
- [RatingsRepository](Team3.Backend/Features/Ratings/RatingsRepository.cs)
- [IRatingsService](Team3.Backend/Features/Ratings/Interfaces/IRatingsService.cs)
- [IRatingsRepository](Team3.Backend/Features/Ratings/Interfaces/IRatingsRepository.cs)

### DTOs

- [CreateRatingRequest](Team3.Backend/Features/Ratings/Dtos/CreateRatingRequest.cs)
- [RatingResponse](Team3.Backend/Features/Ratings/Dtos/RatingResponse.cs)

### Data Model

- [Rating](Team3.Backend/Models/Rating.cs)

The `Rating` entity contains:

- `Id`
- `LearningSessionId`
- `RaterUserId`
- `RatedUserId`
- `Score`
- `Review`
- `CreatedAt`

A unique constraint on `(LearningSessionId, RaterUserId)` prevents a user from submitting more than one rating for the same learning session.
### Skill Verification Requests

Manages requests sent to a mentor to confirm completion of a specific skill.

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/skill-verification-requests` | Send a skill verification request to a mentor. |
| GET | `/api/skill-verification-requests/sent` | Get skill verification requests sent by the current user. |
| GET | `/api/skill-verification-requests/received` | Get skill verification requests received by the current user. |
| PATCH | `/api/skill-verification-requests/{id}` | Accept, reject, or cancel a skill verification request. |

#### Rules
- A user cannot request verification from themselves.
- The requester and mentor must have at least one shared learning session.
- Only one pending verification request can exist for the same requester, mentor, and skill.
- The requester can cancel a pending request.
- The mentor can accept or reject a pending request.
- Accepting a request automatically assigns a score of `5`.
- Notifications are created when a request is received, accepted, rejected, or cancelled.