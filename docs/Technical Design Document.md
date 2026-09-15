# Technical Design Document

## Educational Knowledge & Experience Sharing Platform

---

**1. Overall System Idea**

The proposed system is an educational knowledge and
experience-sharing platform designed to connect students and learners with
people who have relevant skills, experience, or knowledge.

The platform allows users to create profiles that describe
their personal and educational information, while their skills, interests, and
experience are managed through separate relationships.

Based on the user's current learning direction, along with
relevant skills, interests, and experience, the system helps users discover
suitable people for knowledge and experience sharing. Users can also search for
relevant users, skills, and learning areas.

Users can browse other users' profiles and send connection
requests to people who may help them learn or exchange knowledge, subject to
the platform's matching and connection rules. After connecting, users can
communicate and agree to support each other by explaining topics, sharing
experience, or helping with their current learning direction.

The platform is designed to encourage focused and gradual
learning. Users have a limited number of connections so that they can
concentrate on their current learning direction rather than continuously
switching between different areas. As users progress, the system can gradually
recommend people from complementary or other relevant learning areas.

In addition to user connections, the platform supports
educational content publishing and interaction, allowing users to share useful
knowledge and engage with educational posts. The system also includes ratings,
points, and skill verification through mentor endorsement to encourage
knowledge sharing and meaningful participation.

Overall, the platform aims to provide a focused environment
where learners can discover relevant people, exchange educational knowledge and
experience, and gradually expand their learning network.

---

# 2. Backend Architecture

## 2.1 Architecture Overview

The backend follows a **Layered Architecture** organized using
a **Feature-Based project structure**.

The main architecture layers are:

1. API Layer
2. Business Logic / Application Layer
3. Data Access Layer
4. Database

The Data Access Layer uses the **Repository Pattern**, together
with **Entity Framework Core and DbContext**, to separate database
access from business logic.

The general request flow is:

```text
CLIENT
```

```text
   │
```

```text
   │ HTTP Request
```

```text
   ▼
```

```text
┌────────────────────┐
```

```text
│     API Layer      │
│     Controllers    │
└─────────┬──────────┘
```

```text
          │
```

```text
          ▼
```

```text
┌────────────────────┐
```

```text
│   Business Logic   │
│      Services      │
└─────────┬──────────┘
```

```text
          │
```

```text
        ▼
```

```text
┌────────────────────┐
```

```text
│  Data Access Layer │
│    Repositories    │
└─────────┬──────────┘
```

```text
          │
```

```text
          ▼
```

```text
┌────────────────────┐
```

```text
│   EF Core/DbContext│
└─────────┬──────────┘
```

```text
          │
```

```text
          ▼
```

```text
┌────────────────────┐
```

```text
│      Database      │
└────────────────────┘
```

Firebase Authentication is integrated with the backend as the external
authentication service. It authenticates users and provides Firebase ID tokens,
while PostgreSQL stores the application's local `User` and business data. The Firebase account and the
local `User` are linked
through `firebase_uid`.

The project structure is organized by features, with related controllers,
services, DTOs, and validators grouped together where appropriate.

## 2.2 Main Architecture Layers

### API Layer

The API Layer is responsible for receiving HTTP requests and returning HTTP
responses.

It contains Controllers that:

- Receive client requests.
- Handle request binding and basic request validation results.
- Call the appropriate Services.
- Return suitable HTTP responses.

Controllers should not contain the main business rules or direct database
access.

### Business Logic / Application Layer

The Business Logic / Application Layer contains Services responsible for
application operations and business rules.

Services:

- Process application operations.
- Apply business rules.
- Coordinate different components.
- Use Repositories to access required data.

The main business decisions should remain inside Services rather than
Controllers or database-access classes.

### Data Access Layer

The Data Access Layer uses **Repositories, Entity Framework Core, and
DbContext** to communicate with the database while keeping database
access separate from business logic.

Repositories provide an abstraction for database operations and are used by
Services instead of accessing `DbContext`
directly.

Entity Framework Core and `DbContext`
are responsible for mapping application entities to the relational database and
executing database operations against PostgreSQL.

### Database

The system uses **PostgreSQL** as the relational database.

The database stores the application's persistent data, including:

- Users
- Profiles
- Learning Directions
- Progress
- Educational Content
- Connections
- Connection Requests
- Conversations
- Messages
- Learning Sessions
- Ratings
- Notifications
- Skills
- Experiences
- Interests
- Content interactions
- Junction tables

### Authentication Flow

Firebase Authentication operates as an external service integrated with the
backend:

```text
User
```

```text
   ↓
```

```text
Firebase Authentication
```

```text
   ↓
```

```text
Firebase ID Token
```

```text
   ↓
```

```text
Backend API
```

```text
   ↓
```

```text
Token Verification
```

```text
   ↓
```

```text
Identify Local User Using firebase_uid
```

```text
   ↓
```

```text
Controller
```

```text
   ↓
```

```text
Service
```

```text
   ↓
```

```text
Repository
```

```text
   ↓
```

```text
DbContext / EF Core
```

```text
   ↓
```

```text
PostgreSQL Database
```

Firebase Authentication is not considered an application architecture layer
and is not represented as a database entity in the ERD.

### How the Layers Work Together

```text
Client
```

```text
   ↓
```

```text
Controller
```

```text
   ↓
```

```text
Service
```

```text
   ↓
```

```text
Repository
```

```text
   ↓
```

```text
DbContext / EF Core
```

```text
   ↓
```

```text
PostgreSQL Database
```

Each component has a clear responsibility, reducing unnecessary direct
dependencies and keeping business logic, API handling, and data access
separated.

**2.3 Supporting Components**

In addition to the main architecture layers, the backend
includes supporting components that help implement, secure, configure,
document, and test the system.

| **Component**           | **Responsibility**                                                                                                                              |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| Controllers             | Receive HTTP requests, handle request binding and validation results, and return HTTP responses.                                                |
| Services                | Implement application operations and business rules.                                                                                            |
| Repositories            | Provide an abstraction for database operations and separate data access from business logic.                                                    |
| Entities                | Represent the main domain entities used by the application and mapped to the database through Entity Framework Core.                            |
| DTOs                    | Define the data exchanged between the API and its clients without exposing internal entity structures unnecessarily.                            |
| Validation              | Validate incoming data and request models before application processing.                                                                        |
| Middleware              | Handle cross-cutting concerns such as authentication token processing, global exception handling, and request-related processing.               |
| Authentication          | Verify user identity through Firebase Authentication integrated with the Backend.                                                               |
| Authorization           | Determine whether an authenticated user is allowed to perform a requested operation based on ownership, permissions, and business rules.        |
| Firebase Authentication | Provide external user authentication and issue Firebase ID tokens that the Backend verifies to identify authenticated users.                    |
| Logging                 | Record application events, errors, and useful diagnostic information for monitoring and troubleshooting.                                        |
| Configuration           | Manage application settings such as database connection settings and Firebase configuration, while keeping sensitive values out of source code. |
| EF Core Migrations      | Manage and apply changes to the PostgreSQL database schema.                                                                                     |
| OpenAPI / Swagger       | Document the API and provide an interface for exploring and testing API endpoints.                                                              |
| Unit Testing            | Test individual components and business logic in isolation.                                                                                     |
| Integration Testing     | Test the interaction between multiple backend components and external dependencies where applicable.                                            |

## 2.4 Request Flow

For an authenticated request, the general flow is:

```text
User
   ↓
Email + Password
   ↓
Firebase Authentication
   ↓
Firebase ID Token
   ↓
Backend API
   ↓
Token Verification
   ↓
Extract Firebase UID
   ↓
Identify Local User
   ↓
Controller
   ↓
Service
   ↓
Repository
   ↓
DbContext / EF Core
   ↓
PostgreSQL Database
```

The request is processed through the following steps:

1. The user registers or logs in using Email and Password.
2. Firebase Authentication handles the authentication process.
3. Firebase provides an ID Token after successful authentication.
4. The client sends the authenticated request with the Firebase ID Token.
5. The Backend verifies the Firebase ID Token.
6. The Backend extracts the Firebase UID from the verified ID Token and uses it to identify the corresponding local `User`.
7. The Controller receives the request and passes the required data to the Service.
8. The Service applies the required business rules.
9. The Service uses the appropriate Repository to access or modify the required data.
10. The Repository uses `DbContext` and Entity Framework Core to communicate with the PostgreSQL database.
11. The result is returned through the Service and Controller to the client.

## 2.5 Project Organization

The backend follows a **Feature-Based**
organization while keeping shared infrastructure and cross-cutting components
separated.

A proposed project structure is:

```text
Project
│
├── Features
│   ├── Users
│   │   ├── Controller
│   │   ├── Service
│   │   ├── DTOs
│   │   └── Validators
│   │
│   ├── EducationalContent
│   │   ├── Controller
│   │   ├── Service
│   │   ├── DTOs
│   │   └── Validators
│   │
│   └── Connections
│       ├── Controller
│       ├── Service
│       ├── DTOs
│       └── Validators
│
├── Repositories
│   ├── Interfaces
│   └── Implementations
│
├── Entities
│
├── Data
│   └── Migrations
│
├── Middleware
│
└── Tests
    ├── Unit
    └── Integration
```

The feature folders shown above represent the initial organization of the
project. Additional features may be added as the system is developed and their
responsibilities become more clearly defined.

Shared components such as repositories, entities, middleware, database
migrations, and tests remain separated from the feature-specific structure.

**2.6 Important
Architecture Decisions**

**The following decisions
define the main architectural and technical choices for the Backend system.**

| **Decision**                | **Selected Approach**                                   | **Reason**                                                                                                                                        |
| --------------------------- | ------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Architecture Style**      | **Layered Architecture**                                | **Separates responsibilities between the API, business logic, data access, and database.**                                                        |
| **Project Organization**    | **Feature-Based**                                       | **Keeps related functionality organized together and allows the project to grow by adding features as the system develops.**                      |
| **Business Logic**          | **Services**                                            | **Keeps application operations and business rules separate from Controllers and data access.**                                                    |
| **Data Access**             | **Repositories + EF Core + DbContext**                  | **Separates database access from business logic and provides a clear data-access structure.**                                                     |
| **Repository Pattern**      | **Used**                                                | **Provides an abstraction over database operations and makes Services easier to unit test through repository mocking.**                           |
| **API Data Transfer**       | **DTOs**                                                | **Prevents unnecessary exposure of database entities through the API and defines the data exchanged with clients.**                               |
| **Validation**              | **Feature-Based**                                       | **Keeps validation rules close to the related feature and its request models.**                                                                   |
| **Authentication**          | **Firebase Authentication integrated with the Backend** | **Firebase handles user authentication while PostgreSQL stores the application's local User and business data.**                                  |
| **Authentication Method**   | **Email + Password**                                    | **Provides the standard account registration and login flow through Firebase Authentication.**                                                    |
| **Authentication Token**    | **Firebase ID Token**                                   | **Allows the Backend to verify the authenticated Firebase user.**                                                                                 |
| **User Identity Reference** | **Firebase UID**                                        | **Links the Firebase authenticated account to the corresponding local User record.**                                                              |
| **Authorization**           | **ASP.NET Core Authorization**                          | **Controls access to protected Backend operations after the user's identity has been authenticated.**                                             |
| **Error Handling**          | **Global Exception Handling Middleware**                | **Provides consistent handling of unexpected application errors.**                                                                                |
| **Database Changes**        | **EF Core Migrations**                                  | **Manages database schema changes over time.**                                                                                                    |
| **API Documentation**       | **OpenAPI / Swagger**                                   | **Documents the API and provides an interface for exploring and testing API endpoints.**                                                          |
| **Testing**                 | **Unit + Integration Testing**                          | **Verifies individual components in isolation and tests the interaction between multiple backend components.**                                    |
| **Logging**                 | **ASP.NET Core Logging**                                | **Supports application monitoring, diagnostics, and troubleshooting.**                                                                            |
| **Configuration**           | **Application Configuration**                           | **Keeps environment-specific settings separate from application logic and allows configuration to be managed appropriately across environments.** |

# 2.7 Architecture Summary

The backend architecture can be sum`marized as:

```text
┌──────────────────────┐
│       API Layer      │
│      Controllers     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Business Logic Layer │
│       Services       │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│  Data Access Layer   │
│     Repositories     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│   EF Core / DbContext│
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ PostgreSQL Database  │
└──────────────────────┘
```

Firebase Authentication is integrated with the application as the external
authentication service, while PostgreSQL stores the application's local `User` and business data.

---

3. **3.1 User & Profile Management**

The User component represents the application-level user
account, while the Profile component stores the user's public and educational
information.

Firebase Authentication manages authentication, while the
local User entity stores the application-level user record and its Firebase
UID.

Skills, interests, and experience are managed through
separate relationships rather than being stored as direct profile fields.

---

**3.2 Onboarding**

After registering, the user completes an Onboarding step
before accessing the rest of the platform.

During Onboarding, the user provides their current Learning
Direction, along with their initial Skills and Interests. This information is
required because it forms the basis for the platform's matching logic and
personalized recommendations.

Onboarding does not introduce a separate database entity. It
uses the same Profile, Skill, and Interest relationships described elsewhere in
this document; the platform enforces the order in which this information is
collected.

A user cannot access the main platform features (such as the
Home feed or Recommendations) until Onboarding is completed.

---

**3.3 Learning Direction / Track**

The Learning Direction component represents the user's
current primary learning focus.

Each user has one current learning direction at a time.

The learning direction is separate from the user's skills
because a user may have several skills while currently focusing on one specific
area.

# &#x20;

---

**3.4 Progress**

The Progress component represents a user's development
within a learning direction.

Progress is stored separately from the user's current
learning direction so that previous progress records can be preserved if the
user changes their current direction later.

---

**3.5 Educational Content & Knowledge Sharing**

Users can publish educational content to share knowledge and
experience.

Supported content types include:

- Text
- Images
- Videos
- Files / Documents
- External Links

Meetings are not treated as content. They are handled
separately through the Learning Session component.

---

**3.6 Content Interaction**

Users can interact with educational content through:

- Like
- Comment
- Share
- Save
- Repost

A Share represents sharing content with others or outside
the platform and does not create a new post on the user's profile.

A Repost republishes the original content on the user's
profile while keeping a reference to the original content.

A Save allows the user to privately save content for later.

Comments support replies.
**3.7 User Connections**

Users can browse other users' profiles, but they cannot send
connection requests to any user without being subject to the platform's
suitability and matching rules.

Connections are intended for educational and
knowledge-sharing purposes.

The system also applies a connection limit so users can
maintain a focused learning network. As an optional feature, users may exchange
earned points for additional connection attempts beyond this limit.

---

**3.8 Connection Requests**

Users can send connection requests to suitable users.

A connection request can have the following states:

- Pending
- Accepted
- Rejected
- Cancelled

When a request is accepted, a Connection is created.

A sender can cancel a pending request.

A rejected request may allow another request later after a
temporary waiting period. The exact waiting period is a business rule to be
defined later.

---

**3.9 Recommendations**

The Recommendation component helps users discover relevant
people to connect with for knowledge and experience sharing.

Matching is primarily based on shared Learning Direction and
Skills between users. Other profile information such as Interests, Experience,
and Progress may be used to further refine the results.

The matching logic is developed and maintained by the AI
team, while the Backend exposes the required data (Learning Direction, Skills,
Interests, Experience, Progress) and provides the API endpoint through which
recommendations are retrieved.

Recommendations represent the output of the matching logic
and are not currently modeled as an independent database entity.

---

**3.10 Search & Discovery**

The Search component allows users to search for:

- Users and profiles
- Skills
- Educational content
- Topics
- Learning areas

Search is user-driven, while recommendations are
system-driven.

---

**3.11 Messaging & Communication**

Connected users can communicate through conversations and
messages.

Conversations follow an email-style structure rather than a
real-time chat format. Each conversation may have a subject and contains a
thread of messages exchanged between the two connected users.

Messaging can be used for:

- Asking questions
- Discussing learning topics
- Sharing knowledge and experience
- Planning learning sessions

Messaging is available between connected users.

---

**3.12 Learning Sessions / Meetings**

Learning Sessions allow connected users to organize
educational meetings.

A session may include:

- Title
- Description
- Scheduled date and time
- Meeting link
- Status

The platform organizes the session but does not provide
built-in video conferencing.

External platforms such as Google Meet or Zoom may be used
for the actual meeting.

---

**3.13 Ratings & Points**

After a real knowledge-sharing experience, users can rate
each other.

Ratings use a score from 1 to 5 and may include a written
review.

A rating cannot be edited after submission.

Points are used to encourage meaningful participation and
contribution. They may be awarded for activities such as:

- Helping other users
- Completing learning sessions
- Publishing educational content
- Receiving positive ratings
- Participating in useful discussions

The exact points calculation rules will be defined later.

---

**3.14 Skill Verification**

The Skill Verification component allows a user to request
confirmation that they have completed a specific skill from a mentor.

The user sends a verification request to a suitable mentor.
The mentor reviews the request and responds with a rating, and may optionally
include a written note.

This process is separate from the Rating submitted after a
Learning Session, since it specifically confirms skill completion rather than
evaluating a general knowledge-sharing experience.

---

**3.15 Leaderboard**

The Leaderboard component displays a ranked list of users
based on their accumulated points.

Each entry shows the user's name, points, and current
Learning Direction.

The Leaderboard is a read-only view built from existing User
and Points data and does not introduce a separate data-modifying entity.

---

**3.16 Notifications**

The Notification component provides alerts related to
important activities such as:

- Connection requests
- Accepted or rejected requests
- Comments
- Reposts
- Ratings
- Skill verification requests and responses
- Important learning activities
- Meeting activities

Notifications are alerts and are not considered a
communication channel.

---

## 3.17 Overall Component Relationship

The main learning and connection flow can be represented as:

```text id="a7k2m1"
User & Profile
      │
      ├── Learning Direction
      ├── Skills
      ├── Interests
      ├── Experience
      ├── Progress
      └── Profile Information
               │
               ▼
      Matching & Recommendations
               │
               ▼
          Relevant Users
               │
               ▼
        Connection Request
               │
               ▼
           Connection
               │
               ▼
           Messaging
               │
               ▼
       Learning Session
               │
          ┌────┴────┐
          ▼         ▼
       Rating     Points
```

Educational content follows a separate interaction flow:

```text id="m4p9x2"
Educational Content
        ↓
Content Interaction
   ├── Like
   ├── Comment
   ├── Share
   ├── Save
   └── Repost
```

Search and Notifications support the overall platform experience.

---

**4. Main Entities**

The main domain entities are:

1. User
2. Profile
3. LearningDirection
4. Progress
5. EducationalContent
6. Like
7. Comment
8. Share
9. Save
10. Repost
11. Connection
12. ConnectionRequest
13. Conversation
14. Message
15. LearningSession
16. Rating
17. Notification
18. Skill
19. Experience
20. Interest
21. SkillVerificationRequest
22. **PointsTransaction**

The following concepts are not currently modeled as independent main
entities:

- Recommendation — output of the matching logic, not an independent business entity.
- Leaderboard — a read-only ranked view generated from existing User and Points data, not a separate entity.
- Topic — associated with content or learning activities but not currently modeled as an independent entity.
- Text, Image, Video, File, and External Link — content types under EducationalContent.
- Authentication infrastructure managed by Firebase — external authentication functionality and not a main business entity.
- N:N relationships are implemented through junction tables where required.

# 5. Relationships Between Entities

## 5.1 User ↔ Profile

**Relationship:** 1:1

Every User has exactly one Profile, and every Profile belongs to exactly one
User.

`Profile.user_id` is a
unique foreign key.

---

## 5.2 LearningDirection ↔ User

**Relationship:** 1:N

One LearningDirection can be associated with many Users.

Each User has exactly one current LearningDirection at a time.

A user may change their current direction later, while previous progress
records are preserved separately.

---

## 5.3 User ↔ Progress

**Relationship:** 1:N

One User can have multiple Progress records.

Each Progress record belongs to one User.

---

## 5.4 LearningDirection ↔ Progress

**Relationship:** 1:N

One LearningDirection can have Progress records for many Users.

Each Progress record belongs to one specific LearningDirection.

This allows progress to be preserved for previous learning directions.

---

## 5.5 User ↔ EducationalContent

**Relationship:** 1:N

One User can publish many EducationalContent records.

Each EducationalContent record has exactly one original author.

Sharing or reposting content does not change the original author.

---

## 5.6 User ↔ Like

**Relationship:** 1:N

One User can create many Like records.

Each Like belongs to one User.

---

## 5.7 EducationalContent ↔ Like

**Relationship:** 1:N

One EducationalContent item can have many Like records.

Each Like belongs to one EducationalContent item.

A user cannot have more than one active Like on the same content.

A unique constraint is applied to:

```text
(UserId, EducationalContentId)
```

---

## 5.8 EducationalContent ↔ Comment

**Relationship:** 1:N

One EducationalContent item can have many Comments.

Each Comment belongs to one EducationalContent item.

---

## 5.9 User ↔ Comment

**Relationship:** 1:N

One User can create many Comments.

Each Comment is authored by one User.

---

## 5.10 Comment ↔ Comment

**Relationship:** 1:N self-referencing

A Comment can optionally have one parent Comment.

A parent Comment can have many replies.

`ParentCommentId` is
nullable for top-level comments.

The design supports comment replies without requiring a separate Reply
entity.

---

## 5.11 User ↔ Share

**Relationship:** 1:N

One User can create many Share actions.

Each Share belongs to one User.

---

## 5.12 EducationalContent ↔ Share

**Relationship:** 1:N

One EducationalContent item can have many Share actions.

A Share represents a sharing event and may occur multiple times.

---

## 5.13 User ↔ Save

**Relationship:** 1:N

One User can save many EducationalContent items.

Each Save belongs to one User.

A user can save the same content only once at a time.

---

## 5.14 EducationalContent ↔ Save

**Relationship:** 1:N

One EducationalContent item can be saved by many Users.

A unique constraint is applied to:

```text
(UserId, EducationalContentId)
```

---

## 5.15 User ↔ Repost

**Relationship:** 1:N

One User can create many Reposts.

Each Repost belongs to one User.

---

## 5.16 EducationalContent ↔ Repost

**Relationship:** 1:N

One EducationalContent item can be reposted by many Users.

A user can repost the same content only once.

The Repost keeps a reference to the original EducationalContent.

---

## 5.17 User ↔ Connection

**Relationship:** Self-referencing many-to-many, implemented
through Connection

A Connection represents a mutual relationship between exactly two different
Users.

Each User can have many Connections.

The database must prevent:

- Self-connections
- Duplicate connections
- Duplicate reverse pairs

For example, if User A is connected to User B, another connection between
User B and User A must not be created.

---

## 5.18 User ↔ ConnectionRequest

**Relationship:** 1:N

A User can send many connection requests and can receive many connection
requests.

The relationship uses two roles:

- Sender
- Receiver

The request has a status:

- Pending
- Accepted
- Rejected
- Cancelled

---

## 5.19 Connection ↔ Conversation

**Relationship:** 1:0..1

A Connection can have zero or one Conversation.

Each Conversation belongs to exactly one Connection.

---

## 5.20 Conversation ↔ Message

**Relationship:** 1:N

One Conversation can contain many Messages.

Each Message belongs to one Conversation.

---

## 5.21 User ↔ Message

**Relationship:** 1:N as sender

One User can send many Messages.

Each Message has exactly one sender.

A receiver field is not required because the receiver can be determined
through the Conversation and its Connection.

---

## 5.22 Connection ↔ LearningSession

**Relationship:** 1:0..N

One Connection can have zero or many Learning Sessions.

Each Learning Session belongs to one Connection.

---

## 5.23 User ↔ LearningSession

**Relationship:** 1:N through Connection

A User can participate in many Learning Sessions.

Each Learning Session has exactly two participants.

The participants are determined through the associated Connection.

Therefore, LearningSession does not store separate participant User IDs.

---

## 5.24 User ↔ Rating

**Relationship:** 1:N as rater and 1:N as rated user

A User can give many Ratings and can receive many Ratings.

Ratings represent person-to-person evaluation after a real knowledge-sharing
experience.

---

## 5.25 LearningSession ↔ Rating

**Relationship:** 1:0..2

A Learning Session can have zero, one, or two Ratings.

Each participant may submit one Rating for the other participant.

A unique constraint is applied to:

```text
(LearningSessionId, RaterUserId)
```

Ratings cannot be edited after submission.

---

## 5.26 User ↔ Skill

**Relationship:** N:N

A User can have many Skills.

A Skill can belong to many Users.

This relationship is implemented using the `UserSkill` junction table.

---

## 5.27 User ↔ Experience

**Relationship:** 1:N

One User can have many Experiences.

Each Experience belongs to one User.

This relationship is implemented using `Experience.user_id`.

Experiences are user-managed records and are not shared records between
users.

---

## 5.28 User ↔ Interest

**Relationship:** N:N

A User can have many Interests.

An Interest can belong to many Users.

This relationship is implemented using the `UserInterest` junction table.

---

## 5.29 LearningDirection ↔ Skill

**Relationship:** N:N

A LearningDirection can be associated with many Skills.

A Skill can be associated with many Learning Directions.

This relationship is implemented using the `LearningDirectionSkill` junction table.

---

## 5.30 LearningDirection ↔ Interest

**Relationship:** N:N

A LearningDirection can be associated with many Interests.

An Interest can be associated with many Learning Directions.

This relationship is implemented using the `LearningDirectionInterest` junction table

## 5.31 EducationalContent ↔ Skill

**Relationship:** N:N

EducationalContent can be associated with multiple Skills.

A Skill can be associated with multiple EducationalContent items.

This relationship is implemented using the `EducationalContentSkill` junction table.

---

## 5.32 EducationalContent ↔ Interest

**Relationship:** N:N

EducationalContent can be associated with multiple Interests.

An Interest can be associated with multiple EducationalContent items.

This relationship is implemented using the `EducationalContentInterest` junction table.

---

## 5.33 EducationalContent ↔ LearningDirection

**Relationship:** N:N

EducationalContent can be relevant to multiple Learning Directions.

A LearningDirection can have multiple EducationalContent items.

This relationship is implemented using the `EducationalContentLearningDirection` junction table.

---

## 5.34 User ↔ Notification

**Relationship:** 1:N

One User can receive many Notifications.

Each Notification belongs to one User.

---

## 5.35 User ↔ SkillVerificationRequest

**Relationship:** 1:N as requester and 1:N as mentor

A User can send many skill verification requests and can receive many as a mentor.

The relationship uses two roles:

- Requester
- Mentor

---

## 5.36 Skill ↔ SkillVerificationRequest

**Relationship:** 1:N

One Skill can be the subject of many verification requests.

Each verification request is associated with exactly one Skill.

---

## 5.37 User ↔ PointsTransaction

**Relationship:** 1:N

One User can have many PointsTransaction records.

Each PointsTransaction belongs to one User and represents a single points-earning or points-spending event.

---

# 6. Entity and Relationship Summary

| **Entity**                   | **Main Purpose**                                                  |
| ---------------------------- | ----------------------------------------------------------------- |
| User                         | Application-level user record and Firebase identity reference     |
| Profile                      | Personal and educational profile information                      |
| LearningDirection            | User's current learning focus                                     |
| Progress                     | Development within learning directions                            |
| EducationalContent           | Educational knowledge-sharing content                             |
| Like                         | Content like action                                               |
| Comment                      | Content comments and replies                                      |
| Share                        | Content sharing action                                            |
| Save                         | Private content save action                                       |
| Repost                       | Content republication action                                      |
| Connection                   | Mutual relationship between two users                             |
| ConnectionRequest            | Request to establish a connection                                 |
| Conversation                 | Communication space between connected users                       |
| Message                      | Messages exchanged in a conversation                              |
| LearningSession              | Organized learning meeting                                        |
| Rating                       | Evaluation after a learning experience                            |
| Notification                 | User activity alerts                                              |
| Skill                        | Reusable skill definition                                         |
| Experience                   | Reusable experience definition                                    |
| Interest                     | Reusable interest definition                                      |
| **SkillVerificationRequest** | **Request to a mentor to confirm completion of a specific skill** |
| PointsTransaction            | Log of points earned or spent by a user                           |

---

# 7. Database Design

## 7.1 Database Technology

The system uses PostgreSQL as the relational database management system and Entity Framework Core as the Object-Relational Mapper (ORM).

The database design uses:

- PostgreSQL for persistent application and business data.
- Entity Framework Core for database access and object-relational mapping.
- UUID values as primary keys for the main entities.
- Foreign keys to maintain relationships between related tables.
- EF Core Migrations to manage database schema changes.
- Firebase Authentication as the external authentication service.

Firebase Authentication is responsible for user authentication and provides a Firebase UID for each authenticated user. The PostgreSQL database stores the corresponding local User record, including the Firebase UID, together with the application's business data. User passwords are not stored in the PostgreSQL database.

---

## 7.2 Main Database Tables

### User

| **Column**            | **Type** | **Constraints / Purpose**                                                                                                                                                         |
| --------------------- | -------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| id                    | UUID     | Primary Key                                                                                                                                                                       |
| firebase_uid          | VARCHAR  | NOT NULL, UNIQUE; identifies the user in Firebase Authentication                                                                                                                  |
| learning_direction_id | UUID     | NOT NULL, Foreign Key → LearningDirection.id                                                                                                                                      |
| points                | INTEGER  | NOT NULL, DEFAULT 0; running total of points earned by the user _(subject to confirmation — may later move to a separate transaction table if a full points history is required)_ |

Each local User record is linked to one Firebase account through firebase_uid.

---

### Profile

| **Column** | **Type**  | **Constraints / Purpose**     |
| ---------- | --------- | ----------------------------- |
| id         | UUID      | Primary Key                   |
| user_id    | UUID      | Foreign Key → User.id, UNIQUE |
| first_name | VARCHAR   | NOT NULL                      |
| last_name  | VARCHAR   | NOT NULL                      |
| bio        | TEXT      | NULL                          |
| university | VARCHAR   | NULL                          |
| updated_at | TIMESTAMP | NOT NULL                      |

Each user has one profile.

---

### LearningDirection

| **Column**  | **Type** | **Constraints / Purpose** |
| ----------- | -------- | ------------------------- |
| id          | UUID     | Primary Key               |
| name        | VARCHAR  | NOT NULL, UNIQUE          |
| description | TEXT     | NOT NULL                  |

A Learning Direction represents a learning area that users can select as their current learning focus.

---

### Progress

| **Column**            | **Type**  | **Constraints / Purpose**          |
| --------------------- | --------- | ---------------------------------- |
| id                    | UUID      | Primary Key                        |
| user_id               | UUID      | Foreign Key → User.id              |
| learning_direction_id | UUID      | Foreign Key → LearningDirection.id |
| level                 | VARCHAR   | NOT NULL                           |
| started_at            | TIMESTAMP | NOT NULL                           |
| updated_at            | TIMESTAMP | NOT NULL                           |

A unique constraint should be applied to (user_id, learning_direction_id) so that a user has only one progress record for each learning direction.

Historical progress records can be preserved when the user changes their current learning direction.

### EducationalContent

| **Column**   | **Type**       | **Constraints / Purpose**       |
| ------------ | -------------- | ------------------------------- |
| id           | UUID           | Primary Key                     |
| user_id      | UUID           | NOT NULL, Foreign Key → User.id |
| title        | VARCHAR        | NOT NULL                        |
| description  | TEXT           | NULL                            |
| content_type | VARCHAR / Enum | NOT NULL                        |
| content_url  | TEXT           | NULL                            |
| created_at   | TIMESTAMP      | NOT NULL                        |
| updated_at   | TIMESTAMP      | NOT NULL                        |

Supported content types include: Text, Image, Video, File, External Link.

content_url may be NULL when the content does not require a URL, such as text content.

---

### Like

| **Column**             | **Type**  | **Constraints / Purpose**           |
| ---------------------- | --------- | ----------------------------------- |
| id                     | UUID      | Primary Key                         |
| user_id                | UUID      | Foreign Key → User.id               |
| educational_content_id | UUID      | Foreign Key → EducationalContent.id |
| created_at             | TIMESTAMP | NOT NULL                            |

A unique constraint should be applied to (user_id, educational_content_id) to prevent the same user from liking the same content more than once.

---

### Comment

| **Column**             | **Type**  | **Constraints / Purpose**           |
| ---------------------- | --------- | ----------------------------------- |
| id                     | UUID      | Primary Key                         |
| user_id                | UUID      | Foreign Key → User.id               |
| educational_content_id | UUID      | Foreign Key → EducationalContent.id |
| parent_comment_id      | UUID      | NULL, Foreign Key → Comment.id      |
| content                | TEXT      | NOT NULL                            |
| created_at             | TIMESTAMP | NOT NULL                            |
| updated_at             | TIMESTAMP | NOT NULL                            |

parent_comment_id allows comments to have replies through a self-referencing relationship.

---

### Share

| **Column**             | **Type**  | **Constraints / Purpose**           |
| ---------------------- | --------- | ----------------------------------- |
| id                     | UUID      | Primary Key                         |
| user_id                | UUID      | Foreign Key → User.id               |
| educational_content_id | UUID      | Foreign Key → EducationalContent.id |
| created_at             | TIMESTAMP | NOT NULL                            |

Share represents a sharing action. It is treated as an event, so the same user may share the same content more than once.

---

### Save

| **Column**             | **Type**  | **Constraints / Purpose**           |
| ---------------------- | --------- | ----------------------------------- |
| id                     | UUID      | Primary Key                         |
| user_id                | UUID      | Foreign Key → User.id               |
| educational_content_id | UUID      | Foreign Key → EducationalContent.id |
| created_at             | TIMESTAMP | NOT NULL                            |

A unique constraint should be applied to (user_id, educational_content_id) so that a user has only one active save for the same content.

---

### Repost

| **Column**             | **Type**  | **Constraints / Purpose**           |
| ---------------------- | --------- | ----------------------------------- |
| id                     | UUID      | Primary Key                         |
| user_id                | UUID      | Foreign Key → User.id               |
| educational_content_id | UUID      | Foreign Key → EducationalContent.id |
| created_at             | TIMESTAMP | NOT NULL                            |

A unique constraint should be applied to (user_id, educational_content_id) so that a user cannot repost the same content more than once.

The repost keeps a reference to the original educational content.

---

### Connection

| **Column** | **Type**  | **Constraints / Purpose** |
| ---------- | --------- | ------------------------- |
| id         | UUID      | Primary Key               |
| user_a_id  | UUID      | Foreign Key → User.id     |
| user_b_id  | UUID      | Foreign Key → User.id     |
| created_at | TIMESTAMP | NOT NULL                  |

A Connection represents a mutual relationship between two users. Business rules: no self-connections, no duplicate connections, no reverse duplicate connections.

The database implementation should use an appropriate mechanism to prevent both (user_a_id, user_b_id) and (user_b_id, user_a_id) from representing the same connection.

---

### ConnectionRequest

| **Column**       | **Type**       | **Constraints / Purpose** |
| ---------------- | -------------- | ------------------------- |
| id               | UUID           | Primary Key               |
| sender_user_id   | UUID           | Foreign Key → User.id     |
| receiver_user_id | UUID           | Foreign Key → User.id     |
| status           | VARCHAR / Enum | NOT NULL                  |
| created_at       | TIMESTAMP      | NOT NULL                  |
| updated_at       | TIMESTAMP      | NOT NULL                  |

Supported statuses: Pending, Accepted, Rejected, Cancelled.

Duplicate active connection requests between the same sender and receiver are not allowed.

---

### Conversation

| **Column**    | **Type**  | **Constraints / Purpose**           |
| ------------- | --------- | ----------------------------------- |
| id            | UUID      | Primary Key                         |
| connection_id | UUID      | Foreign Key → Connection.id, UNIQUE |
| created_at    | TIMESTAMP | NOT NULL                            |
| updated_at    | TIMESTAMP | NOT NULL                            |

A Conversation belongs to one Connection, and each Connection can have at most one Conversation. Conversations follow an email-style thread structure without a separate subject line.

---

### Message

| **Column**      | **Type**  | **Constraints / Purpose**     |
| --------------- | --------- | ----------------------------- |
| id              | UUID      | Primary Key                   |
| conversation_id | UUID      | Foreign Key → Conversation.id |
| sender_user_id  | UUID      | Foreign Key → User.id         |
| content         | TEXT      | NOT NULL                      |
| created_at      | TIMESTAMP | NOT NULL                      |
| updated_at      | TIMESTAMP | NOT NULL                      |

Messages can be edited by their sender. The receiver is determined through the associated Conversation and Connection.

---

### LearningSession

| **Column**    | **Type**       | **Constraints / Purpose**   |
| ------------- | -------------- | --------------------------- |
| id            | UUID           | Primary Key                 |
| connection_id | UUID           | Foreign Key → Connection.id |
| title         | VARCHAR        | NOT NULL                    |
| description   | TEXT           | NULL                        |
| scheduled_at  | TIMESTAMP      | NOT NULL                    |
| meeting_url   | TEXT           | NULL                        |
| status        | VARCHAR / Enum | NOT NULL                    |
| created_at    | TIMESTAMP      | NOT NULL                    |
| updated_at    | TIMESTAMP      | NOT NULL                    |

Supported statuses: Scheduled, Completed, Cancelled.
Participants are determined through the associated Connection.

---

### Rating

| **Column**          | **Type**  | **Constraints / Purpose**        |
| ------------------- | --------- | -------------------------------- |
| id                  | UUID      | Primary Key                      |
| learning_session_id | UUID      | Foreign Key → LearningSession.id |
| rater_user_id       | UUID      | Foreign Key → User.id            |
| rated_user_id       | UUID      | Foreign Key → User.id            |
| score               | INTEGER   | NOT NULL, range 1–5              |
| review              | TEXT      | NULL                             |
| created_at          | TIMESTAMP | NOT NULL                         |

A unique constraint should be applied to (learning_session_id, rater_user_id). Ratings are immutable after submission.

---

### SkillVerificationRequest

_(جديد)_

| **Column**        | **Type**       | **Constraints / Purpose**                                                     |
| ----------------- | -------------- | ----------------------------------------------------------------------------- |
| id                | UUID           | Primary Key                                                                   |
| requester_user_id | UUID           | NOT NULL, Foreign Key → User.id                                               |
| mentor_user_id    | UUID           | NOT NULL, Foreign Key → User.id                                               |
| skill_id          | UUID           | NOT NULL, Foreign Key → Skill.id                                              |
| status            | VARCHAR / Enum | NOT NULL (Pending, Accepted, Rejected)                                        |
| score             | INTEGER        | NULL; system-generated automatically when status = Accepted                   |
| note              | TEXT           | NULL; optional note from the mentor, allowed with either Accepted or Rejected |
| created_at        | TIMESTAMP      | NOT NULL                                                                      |
| updated_at        | TIMESTAMP      | NOT NULL                                                                      |

A verification request can only be created between a requester and a mentor who have at least one existing Learning Session together. This eligibility rule is enforced by the Business Logic layer rather than through a direct foreign key relationship. The mentor role is not a separate system role — any user may act as a mentor for another user based on their skills and experience.

---

### Notification

| **Column** | **Type**       | **Constraints / Purpose** |
| ---------- | -------------- | ------------------------- |
| id         | UUID           | Primary Key               |
| user_id    | UUID           | Foreign Key → User.id     |
| type       | VARCHAR / Enum | NOT NULL                  |
| message    | TEXT           | NOT NULL                  |
| is_read    | BOOLEAN        | NOT NULL                  |
| created_at | TIMESTAMP      | NOT NULL                  |

### Skill

| **Column**  | **Type** | **Constraints / Purpose** |
| ----------- | -------- | ------------------------- |
| id          | UUID     | Primary Key               |
| name        | VARCHAR  | NOT NULL, UNIQUE          |
| description | TEXT     | NULL                      |

---

### Experience

| **Column**  | **Type** | **Constraints / Purpose**       |
| ----------- | -------- | ------------------------------- |
| id          | UUID     | Primary Key                     |
| user_id     | UUID     | NOT NULL, Foreign Key → User.id |
| title       | VARCHAR  | NOT NULL                        |
| description | TEXT     | NULL                            |

---

### Interest

| **Column**  | **Type** | **Constraints / Purpose** |
| ----------- | -------- | ------------------------- |
| id          | UUID     | Primary Key               |
| name        | VARCHAR  | NOT NULL, UNIQUE          |
| description | TEXT     | NULL                      |

---

### PointsTransaction

| **Column** | **Type**       | **Constraints / Purpose**                                                                             |
| ---------- | -------------- | ----------------------------------------------------------------------------------------------------- |
| id         | UUID           | Primary Key                                                                                           |
| user_id    | UUID           | NOT NULL, Foreign Key → User.id                                                                       |
| amount     | INTEGER        | NOT NULL; positive for earned points, negative for spent points                                       |
| reason     | VARCHAR / Enum | NOT NULL (e.g. ContentPublished, SessionCompleted, PositiveRatingReceived, ConnectionAttemptPurchase) |
| created_at | TIMESTAMP      | NOT NULL                                                                                              |

A PointsTransaction record is created whenever a user earns or spends points. The User.points column stores the current total and is updated whenever a new transaction is recorded.

---

## 7.3 Many-to-Many Junction Tables

The following relationships require junction tables:

| **Junction Table**                    | **Foreign Keys**                                                                                     | **Primary Key**                                   |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------- |
| `UserSkill`                           | `user_id` → `User.id`, `skill_id` → `Skill.id`                                                       | `(user_id, skill_id)`                             |
| `UserInterest`                        | `user_id` → `User.id`, `interest_id` → `Interest.id`                                                 | `(user_id, interest_id)`                          |
| `LearningDirectionSkill`              | `learning_direction_id` → `LearningDirection.id`, `skill_id` → `Skill.id`                            | `(learning_direction_id, skill_id)`               |
| `LearningDirectionInterest`           | `learning_direction_id` → `LearningDirection.id`, `interest_id` → `Interest.id`                      | `(learning_direction_id, interest_id)`            |
| `EducationalContentSkill`             | `educational_content_id` → `EducationalContent.id`, `skill_id` → `Skill.id`                          | `(educational_content_id, skill_id)`              |
| `EducationalContentInterest`          | `educational_content_id` → `EducationalContent.id`, `interest_id` → `Interest.id`                    | `(educational_content_id, interest_id)`           |
| `EducationalContentLearningDirection` | `educational_content_id` → `EducationalContent.id`, `learning_direction_id` → `LearningDirection.id` | `(educational_content_id, learning_direction_id)` |

Each junction table contains the two foreign keys required to represent the many-to-many relationship. The two foreign keys together form a composite primary key.

`Experience` does **not** require a junction table because an Experience belongs to one User directly through `user_id`.

---

## 7.4 Timestamp Design

The updated_at column is included when the record's own data can be modified.

| **Entity**               | **Timestamp**                |
| ------------------------ | ---------------------------- |
| User                     | —                            |
| Profile                  | updated_at                   |
| LearningDirection        | —                            |
| Progress                 | updated_at                   |
| EducationalContent       | created_at, updated_at       |
| Like                     | created_at                   |
| Comment                  | created_at, updated_at       |
| Share                    | created_at                   |
| Save                     | created_at                   |
| Repost                   | created_at                   |
| Connection               | created_at                   |
| ConnectionRequest        | created_at, updated_at       |
| Conversation             | created_at, last_activity_at |
| Message                  | created_at, updated_at       |
| LearningSession          | created_at, updated_at       |
| Rating                   | created_at                   |
| SkillVerificationRequest | created_at, updated_at       |
| Notification             | created_at                   |
| Skill                    | —                            |
| Experience               | —                            |
| Interest                 | —                            |
| PointsTransaction        | created_at                   |
| Junction Tables          | —                            |

Event-based records such as Like, Share, Save, Repost, and Rating do not require `updated_at` because they are not modified after creation.

`Message` includes `updated_at` because messages can be edited.

`last_activity_at` is used in `Conversation` to record the time of the most recent activity in the conversation. It is updated when a new message is sent or another defined conversation activity occurs.

---

## 7.5 Data Integrity

The database design includes constraints to maintain data integrity.

Important rules include:

- Primary keys use UUID values.
- Foreign keys maintain relationships between tables.
- Required fields use NOT NULL constraints.
- Unique fields use UNIQUE constraints where required.
- User-specific content interactions use unique composite constraints where duplicate actions are not allowed.
- A User cannot connect to themselves.
- Duplicate and reverse duplicate connections are not allowed.
- Duplicate active Connection Requests between the same sender and receiver are not allowed.
- A Conversation belongs to one Connection.
- Learning Sessions are associated with an existing Connection.
- Ratings are linked to actual Learning Sessions.
- A user can submit only one rating per Learning Session as the rater.
- The rater and rated user must be different participants in the associated Learning Session.
- Rating scores must be between 1 and 5.
- Experience records belong directly to their associated User.
- A user has only one progress record for a specific Learning Direction.
- **A Skill Verification Request can only be created between a requester and a mentor who have at least one existing Learning Session together.**
- **A User's points balance is maintained through PointsTransaction records rather than being directly editable.**

## 7.6 Review of Duplicated Data

The database design was reviewed to identify unnecessary duplicated data and ensure that information is stored in the most appropriate tables.

### LearningSession

An initial design considered storing:

```text
connection_id
```

```text
participant_one_id
```

```text
participant_two_id
```

The participant fields were removed because the `Connection` already identifies the two users involved in the session.

The final design stores only:

```text
connection_id
```

The participants of the Learning Session are therefore determined through the associated Connection, avoiding unnecessary duplication of user references.

### Rating

The Rating entity contains:

```text
learning_session_id
```

```text
rater_user_id
```

```text
rated_user_id
```

The rated user can be determined from the Learning Session and the rater because a Learning Session has two participants.

However, `rated_user_id` is intentionally retained to explicitly identify the user being evaluated and to make the rating record self-descriptive.

This is a deliberate design decision rather than accidental duplication.

### Authentication Data

Authentication credentials are managed by Firebase Authentication.

The local database does not duplicate passwords or Firebase authentication credentials.

Only the `firebase_uid` is stored in the local `User` entity to establish the relationship between the authenticated Firebase account and the application's local User record.

---

## 7.7 Initial Database Design Status

The initial database design was prepared based on the identified entities and their relationships.

The design was then reviewed for:

- Primary keys
- Foreign keys
- One-to-one relationships
- One-to-many relationships
- Many-to-many relationships
- Duplicate data
- Data integrity
- Normalization requirements

Based on this review, the identified design issues and unnecessary duplication were addressed before preparing the final database design and ERD.

---

# 8. Database Normalization

The database design was reviewed through First Normal Form (1NF), Second Normal Form (2NF), and Third Normal Form (3NF) to identify and reduce unnecessary data duplication and dependency issues before preparing the final ERD.

## 8.1 First Normal Form (1NF)

The database follows 1NF by ensuring that:

- Each table has a primary key.
- Each column contains a single value.
- Repeating groups are separated into related tables.
- Multiple values such as skills, interests, and experiences are not stored as comma-separated values inside `User` or `Profile`.
- Many-to-many relationships are represented using junction tables.

---

## 8.2 Second Normal Form (2NF)

The database follows 2NF by ensuring that non-key attributes depend on the complete primary key.

Junction tables use composite keys consisting of their two foreign keys and do not contain attributes that depend on only part of the key.

For example:

```text
UserSkill
```

```text
----------------
```

```text
user_id
```

```text
skill_id
```

contains only the relationship between `User` and `Skill`.

---

## 8.3 Third Normal Form (3NF)

The database design was reviewed against 3NF by ensuring that attributes are stored with the entity they directly describe and that unnecessary transitive dependencies are avoided.

Examples include:

- Skills are stored in the `Skill` table.
- Interests are stored in the `Interest` table.
- Experiences are stored in the `Experience` table.
- Learning Directions are stored separately.
- Profile information is stored separately from `User`.
- Authentication credentials are managed by Firebase Authentication, while the local database stores only the `firebase_uid` required to link the Firebase account to the local User record.

The design also avoids unnecessary duplication in `LearningSession` by using the associated `Connection` to determine its two participants.

The `rated_user_id` field in `Rating` is intentionally retained as a design decision to explicitly identify the evaluated user, even though the evaluated participant can be determined from the associated Learning Session and the rater.

This field is therefore treated as intentional denormalization rather than accidental duplication.

---

## 8.4 Normalization Result

After reviewing 1NF, 2NF, and 3NF, the database structure was considered sufficiently normalized for the current system requirements.

The final design reduces unnecessary duplication, keeps relationships clear, and supports the required business operations.

The `rated_user_id` field in `Rating` is intentionally retained as a design decision to explicitly identify the evaluated user.

# 9. Final ERD

The final ERD represents the reviewed database structure, including:

·  
Main entities

·  
Primary keys

·  
Foreign keys

·  
One-to-one relationships

·  
One-to-many relationships

·  
Many-to-many relationships

·  
Junction tables

·  
Authentication identity
reference through `firebase_uid`

### Initial ERD

### [https://dbdiagram.io/d/6aa59e87957fec6d5bda2970](https://dbdiagram.io/d/6aa59e87957fec6d5bda2970)

### Final ERD

[https://dbdiagram.io/d/6aa5a54b957fec6d5bda476e](https://dbdiagram.io/d/6aa5a54b957fec6d5bda476e)

The final ERD is based on the reviewed and normalized database design
described in this document.

## 11. API Design

The API is organized around the main system resources required by the
Frontend and Backend teams. The design follows RESTful principles, using
resource-based URLs and HTTP methods to represent the required operations.

Not every database entity is exposed as an independent API resource. Some
entities represent operations or system-generated results. For example, Like,
Save, Share, and Repost are handled as operations on Educational Content, while
Recommendations, Search/Discovery, and the Leaderboard are treated as system
capabilities.

---

### 11.1 Main API Resources

| **API Resource**                   | **Purpose**                                                                                             |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------- |
| Profiles                           | Manages &#x20; the public and educational information displayed for each user.                          |
| Educational &#x20; Content         | Allows &#x20; users to publish, view, and manage educational content and its interactions.              |
| Comments                           | Supports &#x20; comments and replies on educational content.                                            |
| Connection &#x20; Requests         | Manages &#x20; requests to connect users and their request statuses.                                    |
| Connections                        | Represents &#x20; established connections between users for knowledge sharing and &#x20; communication. |
| Conversations                      | Represents &#x20; email-style conversations between connected users.                                    |
| Messages                           | Manages &#x20; messages exchanged within conversations.                                                 |
| Learning &#x20; Sessions           | Organizes &#x20; knowledge-sharing sessions between connected users.                                    |
| Ratings                            | Stores &#x20; ratings and reviews submitted after eligible learning sessions.                           |
| Skill &#x20; Verification Requests | Manages &#x20; requests sent to a mentor to confirm completion of a specific skill.                     |
| Notifications                      | Provides &#x20; users with alerts about relevant platform activities.                                   |
| Learning &#x20; Directions         | Provides &#x20; the available learning directions that users can select as their current &#x20; focus.  |
| Progress                           | Tracks &#x20; a user's development within a learning direction.                                         |
| Skills                             | Provides &#x20; standardized skills associated with users, learning directions, and content.            |
| Experiences                        | Manages &#x20; users' educational or professional experience information.                               |
| Interests                          | Provides &#x20; standardized interests associated with users, learning directions, and &#x20; content.  |
| Points                             | Tracks &#x20; points earned or spent by a user, including the transaction history.                      |

Recommendations, Search/Discovery, and the Leaderboard are treated as system
capabilities rather than independent resources. Like, Save, Share, and Repost
are operations associated with Educational Content.

### 11.2 Required API Endpoints

#### Profiles

| **Method** | **Endpoint**   | **Purpose**                                                        |
| ---------- | -------------- | ------------------------------------------------------------------ |
| GET        | /profiles/{id} | View &#x20; a public profile.                                      |
| POST       | /profiles      | Create &#x20; the current user's profile.                          |
| PATCH      | /profiles/{id} | Partially &#x20; update the user's own profile.                    |
| PATCH      | /profiles/me   | Update &#x20; the authenticated user's current Learning Direction. |

Design decision: Each user has exactly one profile. The profile cannot be
deleted independently. Deleting the user's account is handled through DELETE
/users/me, which removes the user account and its associated profile according
to the database deletion rules.

The authenticated user's identity is obtained from the Firebase ID token, so
the client does not need to provide the user's ID for /profiles/me.

The GET /profiles/{id} response aggregates related data — including points,
skills, interests, and a session summary — so the Frontend can render a
complete profile view with a single request, without needing separate calls to
skill or interest endpoints for another user's profile.

---

#### Educational Content

| **Method** | **Endpoint**                      | **Purpose**                                                                                                                                                                            |
| ---------- | --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| GET        | /educational-content              | Retrieve educational content. **Supports an optional userId &#x20; query parameter to retrieve content published by a specific user (e.g., when &#x20; viewing that user's profile).** |
| GET        | /educational-content/{id}         | Retrieve a specific content item.                                                                                                                                                      |
| POST       | /educational-content              | Create educational content.                                                                                                                                                            |
| PATCH      | /educational-content/{id}         | Partially update owned content.                                                                                                                                                        |
| DELETE     | /educational-content/{id}         | Delete owned content.                                                                                                                                                                  |
| POST       | /educational-content/{id}/likes   | Like content.                                                                                                                                                                          |
| DELETE     | /educational-content/{id}/likes   | Remove the current user's like.                                                                                                                                                        |
| POST       | /educational-content/{id}/saves   | Save content for the current user.                                                                                                                                                     |
| DELETE     | /educational-content/{id}/saves   | Remove saved content.                                                                                                                                                                  |
| POST       | /educational-content/{id}/reposts | Repost content on the current user's profile.                                                                                                                                          |
| DELETE     | /educational-content/{id}/reposts | Remove the current user's repost.                                                                                                                                                      |
| POST       | /educational-content/{id}/shares  | Record a content sharing action.                                                                                                                                                       |

|     |     |     |     |     |
| --- | --- | --- | --- | --- |

Design decision: Like, Save, Repost, and Share are handled
as operations on Educational Content rather than separate top-level resources.
Like, Save, and Repost represent user-controlled states, while Share represents
a sharing event.

#### Comments

| **Method** | **Endpoint**                              | **Purpose**                                       |
| ---------- | ----------------------------------------- | ------------------------------------------------- |
| GET        | /educational-content/{contentId}/comments | Retrieve &#x20; comments for educational content. |
| POST       | /educational-content/{contentId}/comments | Create &#x20; a comment or reply.                 |
| PATCH      | /comments/{id}                            | Update &#x20; the user's own comment.             |
| DELETE     | /comments/{id}                            | Delete &#x20; the user's own comment.             |

Design decision: Replies use the same Comment resource through
parentCommentId, so a separate Reply resource is not required.

---

#### Connection Requests

| **Method** | **Endpoint**                  | **Purpose**                                   |
| ---------- | ----------------------------- | --------------------------------------------- |
| POST       | /connection-requests          | Send &#x20; a connection request.             |
| GET        | /connection-requests/received | Retrieve &#x20; received connection requests. |
| GET        | /connection-requests/sent     | Retrieve &#x20; sent connection requests.     |
| PATCH      | /connection-requests/{id}     | Change &#x20; the request status.             |

Design decision: Accepting, rejecting, and cancelling a request are status
changes to the same resource and are handled through PATCH.

---

#### Connections

| **Method** | **Endpoint**                   | **Purpose**                                                                                                        |
| ---------- | ------------------------------ | ------------------------------------------------------------------------------------------------------------------ |
| GET        | /connections                   | Retrieve &#x20; the current user's connections.                                                                    |
| GET        | /connections/{id}              | Retrieve &#x20; a specific connection.                                                                             |
| DELETE     | /connections/{id}              | End &#x20; an existing connection.                                                                                 |
| POST       | /connections/attempts/purchase | (Optional/Secondary) &#x20; Exchange points for an additional connection attempt beyond the standard &#x20; limit. |

Design decision: A Connection is created as a result of accepting a
Connection Request, so a separate POST /connections endpoint is not required.
The attempts-purchase endpoint is a secondary feature and may be scheduled
after the core MVP.

---

#### Conversations

| **Method** | **Endpoint**        | **Purpose**                                       |
| ---------- | ------------------- | ------------------------------------------------- |
| GET        | /conversations      | Retrieve &#x20; the current user's conversations. |
| GET        | /conversations/{id} | Retrieve &#x20; a specific conversation.          |

Design decision: Conversations follow an email-style structure and are not
independently created by users through the API — a Conversation is created
automatically the first time a message is sent within a Connection, using the
subject provided with that first message.

#### Messages

| **Method** | **Endpoint**                             | **Purpose**                                         |
| ---------- | ---------------------------------------- | --------------------------------------------------- |
| GET        | /conversations/{conversationId}/messages | Retrieve &#x20; messages in a conversation.         |
| POST       | /conversations/{conversationId}/messages | Send &#x20; a message in a conversation.            |
| PATCH      | /messages/{id}                           | Update &#x20; the authenticated user's own message. |

Design decision: The authenticated user is identified as the sender, while
the conversation identifies the participants. A separate receiverUserId is not
required. When sending the first message in a Connection, the request may
include a subject, which is stored on the Conversation and cannot be changed
afterward. Messages can be edited by their sender; the Backend must verify that
the authenticated user is the owner of the message before allowing the update.

---

#### Learning Sessions

| **Method** | **Endpoint**            | **Purpose**                                        |
| ---------- | ----------------------- | -------------------------------------------------- |
| GET        | /learning-sessions      | Retrieve &#x20; the user's learning sessions.      |
| GET        | /learning-sessions/{id} | Retrieve &#x20; a specific learning session.       |
| POST       | /learning-sessions      | Create &#x20; a learning session for a connection. |
| PATCH      | /learning-sessions/{id} | Update &#x20; session details or status.           |

Design decision: Sessions use statuses such as Scheduled, Completed, and
Cancelled, so cancellation does not require deleting the session.

---

#### Ratings

| **Method** | **Endpoint**                           | **Purpose**                                                |
| ---------- | -------------------------------------- | ---------------------------------------------------------- |
| POST       | /learning-sessions/{sessionId}/ratings | Submit &#x20; a rating after an eligible learning session. |
| GET        | /profiles/{userId}/ratings             | Retrieve &#x20; ratings received by a user.                |

Design decision: The sessionId identifies the Learning Session, while the
authenticated user identifies the rater. The Backend determines the other
participant through the session's Connection.

---

#### Skill Verification Requests

| **Method** | **Endpoint**                          | **Purpose**                                                        |
| ---------- | ------------------------------------- | ------------------------------------------------------------------ |
| POST       | /skill-verification-requests          | Send &#x20; a skill verification request to a mentor.              |
| GET        | /skill-verification-requests/sent     | Retrieve &#x20; requests sent by the current user.                 |
| GET        | /skill-verification-requests/received | Retrieve &#x20; requests received by the current user as a mentor. |
| PATCH      | /skill-verification-requests/{id}     | Accept &#x20; or reject the request, optionally including a note.  |

Design decision: The mentor role is not a separate system role — any user
may act as a mentor for another user based on their skills and experience. A
request can only be created between a requester and a mentor who share at least
one existing Learning Session; the Backend enforces this eligibility rule. When
the mentor accepts the request, the score is generated automatically by the
system rather than entered manually. A note may be included by the mentor with
either an Accept or a Reject response. This process is separate from the Rating
submitted after a Learning Session, since it specifically confirms skill
completion rather than evaluating a general knowledge-sharing experience.

---

#### Notifications

| **Method** | **Endpoint**        | **Purpose**                                         |
| ---------- | ------------------- | --------------------------------------------------- |
| GET        | /notifications      | Retrieve &#x20; notifications for the current user. |
| PATCH      | /notifications/{id} | Update &#x20; the notification read status.         |
| DELETE     | /notifications/{id} | Delete &#x20; a notification.                       |

Design decision: Users can mark notifications as read without deleting them
and can delete notifications they no longer need.

---

#### Learning Directions

| **Method** | **Endpoint**              | **Purpose**                                    |
| ---------- | ------------------------- | ---------------------------------------------- |
| GET        | /learning-directions      | Retrieve &#x20; available learning directions. |
| GET        | /learning-directions/{id} | Retrieve &#x20; a specific learning direction. |

Design decision: Learning Directions are standardized system data and are
retrieved by users rather than created or modified through the regular user
API.

---

#### Progress

| **Method** | **Endpoint**   | **Purpose**                                               |
| ---------- | -------------- | --------------------------------------------------------- |
| GET        | /progress      | Retrieve &#x20; the current user's progress records.      |
| GET        | /progress/{id} | Retrieve &#x20; a specific progress record.               |
| POST       | /progress      | Create &#x20; a progress record for a learning direction. |
| PATCH      | /progress/{id} | Update &#x20; the user's progress level.                  |

Design decision: Progress is stored separately from Learning Direction to
preserve the user's development across learning directions over time.
Skills
Method Endpoint Purpose
GET /skills Retrieve available standardized skills.
GET /skills/{id} Retrieve a specific skill.
GET /profiles/me/skills Retrieve the authenticated user's skills.
PUT /profiles/me/skills/{skillId} Associate an existing skill with the authenticated user.
DELETE /profiles/me/skills/{skillId} Remove a skill association from the authenticated user.
Design decision: Skills are standardized system data. Users associate existing skills with their profiles rather than creating or modifying shared skill definitions. PUT is used for the user-skill association because the operation is idempotent.

---

Experiences
Method Endpoint Purpose
GET /experiences Retrieve the current user's experiences.
GET /experiences/{id} Retrieve a specific experience.
POST /experiences Add a new experience.
PATCH /experiences/{id} Update an existing experience.
DELETE /experiences/{id} Delete an experience.
Design decision: Experiences are user-managed information, so users can create, update, and delete their own records.

---

Interests
Method Endpoint Purpose
GET /interests Retrieve available standardized interests.
GET /interests/{id} Retrieve a specific interest.
GET /profiles/me/interests Retrieve the authenticated user's interests.
PUT /profiles/me/interests/{interestId} Associate an existing interest with the authenticated user.
DELETE /profiles/me/interests/{interestId} Remove an interest association from the authenticated user.
Design decision: Interests are standardized system data. Users associate existing interests with their profiles rather than creating or modifying shared interest definitions. PUT is used for the user-interest association because the operation is idempotent.

---

Points
Method Endpoint Purpose
GET /points/me Retrieve the current user's points balance and transaction history.
Design decision: The points balance is read-only from the client's perspective; it is updated internally whenever a relevant activity (publishing content, completing a session, receiving a positive rating, etc.) or a points-spending action occurs.

---

11.3 Additional API Capabilities
Capability Method Endpoint Purpose
Account Deletion DELETE /users/me Delete the current user's account and associated data according to the database deletion rules.
Recommendations GET /recommendations Retrieve users recommended for the current user, primarily based on shared Learning Direction and Skills.
Search GET /search?query={query}&type={type} Search users, skills, or learning directions. The optional type parameter narrows results to "users", "skills", or "learningDirections"; when omitted, results include all types.
Leaderboard GET /leaderboard Retrieve a ranked list of users based on accumulated points, including name, points, and current Learning Direction.
Recommendations, Search/Discovery, and the Leaderboard are treated as system capabilities rather than independent database resources. Account deletion is handled at the User level because the Profile is associated with the User.

````md
### 11.4 Expected Request Data

The request data for each endpoint is defined below. The authenticated
user's identity is derived from Firebase Authentication where applicable.
Therefore, client applications do not send the authenticated user's ID for
user-owned operations.

#### Profiles

| **Method** | **Endpoint**   | **Expected Request Data**                                                      |
| ---------- | -------------- | ------------------------------------------------------------------------------ |
| GET        | /profiles/{id} | No &#x20; request body. The Profile ID is provided in the URL.                 |
| POST       | /profiles      | firstName, &#x20; lastName, bio, university                                    |
| PATCH      | /profiles/{id} | Any &#x20; of firstName, lastName, bio, university                             |
| PATCH      | /profiles/me   | learningDirectionId                                                            |
| DELETE     | /users/me      | No &#x20; request body. The current user is identified through authentication. |

Example for creating a profile:

json

```json
{
  "firstName": "Yara",
  "lastName": "Kmail",
  "bio": "Computer Systems Engineering student interested in backend development.",
  "university": "Arab American University"
}
```
````

The PATCH /profiles/me endpoint is used to update the authenticated user's
current Learning Direction.

Example:

json

```json
{
  "learningDirectionId": "learning-direction-uuid"
}
```

---

#### Educational Content

| **Method**    | **Endpoint**                      | **Expected Request Data**                                                                                                                                                             |
| ------------- | --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| GET           | /educational-content              | **No request body. Accepts an optional userId query parameter to filter results by author (e.g., /educational-content?userId={id}); when omitted, returns the general content feed.** |
| GET           | /educational-content/{id}         | No request body.                                                                                                                                                                      |
| POST          | /educational-content              | title, description, contentType, contentUrl                                                                                                                                           |
| PATCH         | /educational-content/{id}         | Any of title, description, contentType, contentUrl                                                                                                                                    |
| DELETE        | /educational-content/{id}         | No request body.                                                                                                                                                                      |
| POST / DELETE | /educational-content/{id}/likes   | No request body.                                                                                                                                                                      |
| POST / DELETE | /educational-content/{id}/saves   | No request body.                                                                                                                                                                      |
| POST / DELETE | /educational-content/{id}/reposts | No request body.                                                                                                                                                                      |
| POST          | /educational-content/{id}/shares  | No request body.                                                                                                                                                                      |

Example:

json

```json
{
  "title": "Introduction
to C#",
  "description": "Basic
concepts of C# programming.",
  "contentType": "Video",
  "contentUrl": "[https://example.com/video](https://example.com/video)"
}
```

Supported content types are Text, Image, Video, File, and
ExternalLink. A Text content item may have a null contentUrl.

---

#### Comments

| **Method** | **Endpoint**                              | **Expected Request Data**         |
| ---------- | ----------------------------------------- | --------------------------------- |
| GET        | /educational-content/{contentId}/comments | No request body.                  |
| POST       | /educational-content/{contentId}/comments | content, optional parentCommentId |
| PATCH      | /comments/{id}                            | content                           |
| DELETE     | /comments/{id}                            | No request body.                  |

Example:

json

```json
{
  "content": "This explanation was very helpful."
}
```

For a reply:

json

```json
{
  "content": "I agree, especially the example.",
  "parentCommentId": "comment-uuid"
}
```

---

#### Connection Requests

| **Method** | **Endpoint**                  | **Expected Request Data** |
| ---------- | ----------------------------- | ------------------------- |
| POST       | /connection-requests          | receiverUserId            |
| GET        | /connection-requests/received | No request body.          |
| GET        | /connection-requests/sent     | No request body.          |
| PATCH      | /connection-requests/{id}     | status                    |

Example:

json

```json
{
  "receiverUserId": "user-uuid"
}
```

For a status change:

json

```json
{
  "status": "Accepted"
}
```

Valid statuses are Pending, Accepted, Rejected, and Cancelled. The Backend
validates request eligibility, valid status transitions, duplicate active
requests, self-requests, and applicable connection limits.

---

#### Connections

| **Method** | **Endpoint**                   | **Expected Request Data** |
| ---------- | ------------------------------ | ------------------------- |
| GET        | /connections                   | No request body.          |
| GET        | /connections/{id}              | No request body.          |
| DELETE     | /connections/{id}              | No request body.          |
| POST       | /connections/attempts/purchase | No request body.          |

---

#### Conversations & Messages

| **Method** | **Endpoint**                             | **Expected Request Data**                                                                           |
| ---------- | ---------------------------------------- | --------------------------------------------------------------------------------------------------- |
| GET        | /conversations                           | No request body.                                                                                    |
| GET        | /conversations/{id}                      | No request body.                                                                                    |
| GET        | /conversations/{conversationId}/messages | No request body.                                                                                    |
| POST       | /conversations/{conversationId}/messages | content, optional subject (only used and stored when this is the first message in the conversation) |
| PATCH      | /messages/{id}                           | content                                                                                             |

Example (first message, starting a new conversation):

json

```json
{
  "subject": "Question about React Hooks",
  "content": "Hi, I wanted to ask about..."
}
```

Example (subsequent message):

json

```json
{
  "content": "Thanks for the explanation!"
}
```

The sender is identified through authentication, while conversationId is
provided in the URL. For message updates, the Backend verifies that the
authenticated user is the owner of the message.

```

```

---

**Learning Sessions**

| Method  | Endpoint                  | Expected Request Data                                                |
| ------- | ------------------------- | -------------------------------------------------------------------- |
| `GET`   | `/learning-sessions`      | No request body.                                                     |
| `GET`   | `/learning-sessions/{id}` | No request body.                                                     |
| `POST`  | `/learning-sessions`      | `connectionId`, `title`, `description`, `scheduledAt`, `meetingUrl`  |
| `PATCH` | `/learning-sessions/{id}` | Any of `title`, `description`, `scheduledAt`, `meetingUrl`, `status` |

Example:

```json
{
  "connectionId": "connection-uuid",
  "title": "C# Basics",
  "description": "Introduction to C# OOP concepts.",
  "scheduledAt": "2026-10-15T18:00:00",
  "meetingUrl": "https://meet.google.com/example"
}
```

> `meetingUrl` is optional. Valid session statuses are `Scheduled`, `Completed`, and `Cancelled`.

---

**Ratings**

| Method | Endpoint                                 | Expected Request Data      |
| ------ | ---------------------------------------- | -------------------------- |
| `POST` | `/learning-sessions/{sessionId}/ratings` | `score`, optional `review` |
| `GET`  | `/profiles/{userId}/ratings`             | No request body.           |

Example:

```json
{
  "score": 5,
  "review": "Very helpful session."
}
```

> The `score` must be between 1 and 5. The `review` is optional. The `sessionId` identifies the Learning Session, and the authenticated user is identified as the rater. A rating can only be submitted after an eligible learning session, and each rater can submit only one rating per session.

---

**Skill Verification Requests**

| Method  | Endpoint                                | Expected Request Data                                |
| ------- | --------------------------------------- | ---------------------------------------------------- |
| `POST`  | `/skill-verification-requests`          | `mentorUserId`, `skillId`                            |
| `GET`   | `/skill-verification-requests/sent`     | No request body.                                     |
| `GET`   | `/skill-verification-requests/received` | No request body.                                     |
| `PATCH` | `/skill-verification-requests/{id}`     | `status` (`Accepted` or `Rejected`), optional `note` |

Example (sending a request):

```json
{
  "mentorUserId": "user-uuid",
  "skillId": "skill-uuid"
}
```

Example (mentor responding):

```json
{
  "status": "Accepted",
  "note": "Great understanding of the core concepts."
}
```

> The requester and skill are identified in the request body; the mentor is identified through `mentorUserId`. The Backend verifies that the requester and mentor share at least one existing Learning Session before creating the request. The score is not included in the request — it is generated automatically by the system when the request is accepted.

---

**Notifications**

| Method   | Endpoint              | Expected Request Data |
| -------- | --------------------- | --------------------- |
| `GET`    | `/notifications`      | No request body.      |
| `PATCH`  | `/notifications/{id}` | `isRead`              |
| `DELETE` | `/notifications/{id}` | No request body.      |

Example:

```json
{
  "isRead": true
}
```

> The Backend verifies that the notification belongs to the authenticated user.

---

**Learning Directions**

| Method | Endpoint                    | Expected Request Data |
| ------ | --------------------------- | --------------------- |
| `GET`  | `/learning-directions`      | No request body.      |
| `GET`  | `/learning-directions/{id}` | No request body.      |

---

**Progress**

| Method  | Endpoint         | Expected Request Data          |
| ------- | ---------------- | ------------------------------ |
| `GET`   | `/progress`      | No request body.               |
| `GET`   | `/progress/{id}` | No request body.               |
| `POST`  | `/progress`      | `learningDirectionId`, `level` |
| `PATCH` | `/progress/{id}` | `level`                        |

Example (creating progress):

```json
{
  "learningDirectionId": "learning-direction-uuid",
  "level": "Beginner"
}
```

> The user ID is derived from authentication, and `startedAt` is generated by the Backend.

Example (updating progress):

```json
{
  "level": "Intermediate"
}
```

> `startedAt` remains unchanged while `updatedAt` is updated.

---

**Skills & Interests**

| Method   | Endpoint                              | Expected Request Data |
| -------- | ------------------------------------- | --------------------- |
| `GET`    | `/skills`                             | No request body.      |
| `GET`    | `/skills/{id}`                        | No request body.      |
| `GET`    | `/profiles/me/skills`                 | No request body.      |
| `PUT`    | `/profiles/me/skills/{skillId}`       | No request body.      |
| `DELETE` | `/profiles/me/skills/{skillId}`       | No request body.      |
| `GET`    | `/interests`                          | No request body.      |
| `GET`    | `/interests/{id}`                     | No request body.      |
| `GET`    | `/profiles/me/interests`              | No request body.      |
| `PUT`    | `/profiles/me/interests/{interestId}` | No request body.      |
| `DELETE` | `/profiles/me/interests/{interestId}` | No request body.      |

---

**Experiences**

| Method   | Endpoint            | Expected Request Data  |
| -------- | ------------------- | ---------------------- |
| `GET`    | `/experiences`      | No request body.       |
| `GET`    | `/experiences/{id}` | No request body.       |
| `POST`   | `/experiences`      | `title`, `description` |
| `PATCH`  | `/experiences/{id}` | `title`, `description` |
| `DELETE` | `/experiences/{id}` | No request body.       |

Example:

```json
{
  "title": "Backend Development Intern",
  "description": "Worked on REST APIs and database development."
}
```

---

**Points**

| Method | Endpoint     | Expected Request Data |
| ------ | ------------ | --------------------- |
| `GET`  | `/points/me` | No request body.      |

---

**Additional API Capabilities**

| Method | Endpoint                            | Expected Request Data                                                                                |
| ------ | ----------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `GET`  | `/recommendations`                  | No request body.                                                                                     |
| `GET`  | `/search?query=backend&type=skills` | Query parameters: `query` (required), `type` (optional: `users`, `skills`, or `learningDirections`). |
| `GET`  | `/leaderboard`                      | No request body.                                                                                     |

### 11.5 API Design Principles

The API design follows these principles:

- Resource-based URLs: URLs use nouns representing resources rather than action-based URLs.
- HTTP method semantics: HTTP methods are used according to the intended operation.
- Authenticated user context: User-owned operations derive the current user's identity from Firebase Authentication rather than accepting the user ID from the client.
- Business logic separation: Business rules are enforced in the Business Logic/Application Layer.
- Nested resources: Nested endpoints are used when an operation is strongly related to a parent resource.
- Controlled reference data: Shared reference data such as Skills, Interests, and Learning Directions is not freely modified by regular users.
- Capability-based operations: Recommendations, Search/Discovery, and the Leaderboard are treated as system capabilities, while Like, Save, Share, and Repost are operations associated with Educational Content.
- DTO-based API contract: The API exposes DTOs designed around application and frontend requirements rather than directly exposing the database schema.
- Partial updates: PATCH is used for partial updates and appropriate state changes.
- Ownership and authorization: User-owned resources are protected through authentication and authorization rules.
- Account-level deletion: Account deletion is handled at the User level because each User has one associated Profile.
- Eligibility-based operations: Certain operations, such as Skill Verification Requests, require an existing relationship or history (e.g., a shared Learning Session) between users, enforced by the Backend rather than the client.

---

### 11.6 Expected Response Data

Each API endpoint returns a response appropriate to the requested operation.
Responses are designed around the data required by the client and do not expose
internal database or authentication details unnecessarily.

#### Profiles

| **Method** | **Endpoint**   | **Expected Response Data**                                                                                                                                                                                          |
| ---------- | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| GET        | /profiles/{id} | Profile information including id, userId, firstName, lastName, bio, university, updatedAt, points, skills, interests, and a sessions summary (completedSessionsCount, scheduledSessionsCount). |
| POST       | /profiles      | The created profile including its generated id and profile information.                                                                                                                                      |
| PATCH      | /profiles/{id} | The updated profile including its current information and updatedAt.                                                                                                                                         |
| PATCH      | /profiles/me   | The updated current Learning Direction information.                                                                                                                                                          |
| DELETE     | /users/me      | No response body when 204 No Content is returned.                                                                                                                                                            |

Example response for GET /profiles/{id}:

```json
{
  "id": "profile-uuid",
  "userId": "user-uuid",
  "firstName": "Yara",
  "lastName": "Kmail",
  "bio": "Computer Systems Engineering student.",
  "university": "Arab American University",
  "points": 240,
  "skills": [{ "id": "skill-uuid", "name": "React" }],
  "interests": [{ "id": "interest-uuid", "name": "Backend Development" }],
  "sessions": {
    "completedSessionsCount": 12,
    "scheduledSessionsCount": 3
  },
  "updatedAt": "2026-09-14T10:00:00"
}
```

#### Educational Content

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /educational-content | A collection of educational content items with their main information and relevant interaction data. |
| GET | /educational-content/{id} | The requested content item with its details and relevant interaction information. |
| POST | /educational-content | The created content including its generated id and current information. |
| PATCH | /educational-content/{id} | The updated content including its current information and updatedAt. |
| DELETE | /educational-content/{id} | No response body when 204 No Content is returned. |
| POST/DELETE | /educational-content/{id}/likes | No response body when 204 No Content is returned. |
| POST/DELETE | /educational-content/{id}/saves | No response body when 204 No Content is returned. |
| POST/DELETE | /educational-content/{id}/reposts | No response body when 204 No Content is returned. |
| POST | /educational-content/{id}/shares | No response body when 204 No Content is returned. |

---

#### Comments

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /educational-content/{contentId}/comments | A collection of comments including author information, content, parentCommentId, and timestamps. |
| POST | /educational-content/{contentId}/comments | The created comment including its generated id, author, content, parentCommentId, and createdAt. |
| PATCH | /comments/{id} | The updated comment including its current content and updatedAt. |
| DELETE | /comments/{id} | No response body when 204 No Content is returned. |

---

#### Connection Requests

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| POST | /connection-requests | The created request including id, sender, receiver, status, and timestamps. |
| GET | /connection-requests/received | A collection of received requests with sender information and status. |
| GET | /connection-requests/sent | A collection of sent requests with receiver information and status. |
| PATCH | /connection-requests/{id} | The updated request including its new status and updatedAt. |

---

#### Connections

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /connections | A collection of the current user's connections with relevant user/profile information. |
| GET | /connections/{id} | The requested connection and its associated users. |
| DELETE | /connections/{id} | No response body when 204 No Content is returned. |
| POST | /connections/attempts/purchase | The updated points balance and remaining connection attempts. |

---

#### Conversations

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /conversations | A collection of conversations including subject, participant information, and lastActivityAt. |
| GET | /conversations/{id} | The requested conversation including its subject, participants, and lastActivityAt. |

---

#### Messages

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /conversations/{conversationId}/messages | A collection of messages including sender information, content, and timestamps. |
| POST | /conversations/{conversationId}/messages | The created message including id, sender, content, and timestamps. |
| PATCH | /messages/{id} | The updated message including its current content and updatedAt. |

---

#### Learning Sessions

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /learning-sessions | A collection of the user's learning sessions with session and connection information. |
| GET | /learning-sessions/{id} | The requested session including its details, status, connection, and meeting information. |
| POST | /learning-sessions | The created session including its generated id, details, status, and timestamps. |
| PATCH | /learning-sessions/{id} | The updated session including its current details, status, and updatedAt. |

---

#### Ratings

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| POST | /learning-sessions/{sessionId}/ratings | The submitted rating including score, review, rater, rated user, and createdAt. |
| GET | /profiles/{userId}/ratings | A collection of ratings received by the user, including score, review, rater information, and session reference. |

---

#### Skill Verification Requests

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| POST | /skill-verification-requests | The created request including id, requester, mentor, skill, status, and timestamps. |
| GET | /skill-verification-requests/sent | A collection of sent requests with mentor and skill information, and status. |
| GET | /skill-verification-requests/received | A collection of received requests with requester and skill information, and status. |
| PATCH | /skill-verification-requests/{id} | The updated request including its status, score (if accepted), optional note, and updatedAt. |

---

#### Notifications

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /notifications | A collection of notifications including type, message, read status, and timestamp. |
| PATCH | /notifications/{id} | The updated notification including its current read status. |
| DELETE | /notifications/{id} | No response body when 204 No Content is returned. |

---

#### Learning Directions

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /learning-directions | A collection of available learning directions including id, name, and description. |
| GET | /learning-directions/{id} | The requested learning direction including its details. |

---

#### Progress

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /progress | A collection of the current user's progress records including learning direction, level, and dates. |
| GET | /progress/{id} | The requested progress record including its details and associated learning direction. |
| POST | /progress | The created progress record including id, learning direction, level, and timestamps. |
| PATCH | /progress/{id} | The updated progress record including its current level and updatedAt. |

---

#### Skills

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /skills | A collection of standardized skills including id, name, and description. |
| GET | /skills/{id} | The requested skill including its details. |
| GET | /profiles/me/skills | A collection of skills associated with the current user's profile. |
| PUT | /profiles/me/skills/{skillId} | No response body when 204 No Content is returned. |
| DELETE | /profiles/me/skills/{skillId} | No response body when 204 No Content is returned. |

---

#### Experiences

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /experiences | A collection of the current user's experiences. |
| GET | /experiences/{id} | The requested experience including its details. |
| POST | /experiences | The created experience including its generated id and details. |
| PATCH | /experiences/{id} | The updated experience including its current information. |
| DELETE | /experiences/{id} | No response body when 204 No Content is returned. |

---

#### Interests

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /interests | A collection of standardized interests including id, name, and description. |
| GET | /interests/{id} | The requested interest including its details. |
| GET | /profiles/me/interests | A collection of interests associated with the current user's profile. |
| PUT | /profiles/me/interests/{interestId} | No response body when 204 No Content is returned. |
| DELETE | /profiles/me/interests/{interestId} | No response body when 204 No Content is returned. |

---

#### Points

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| GET | /points/me | The user's current points balance and a collection of transaction records (amount, reason, createdAt). |

---

#### Additional API Capabilities

| **Method** | **Endpoint** | **Expected Response Data** |
| --- | --- | --- |
| DELETE | /users/me | No response body when 204 No Content is returned. |
| GET | /recommendations | A collection of recommended users/profiles with relevant matching information. |
| GET | /search?query={query}&type={type} | Search results based on the requested query, optionally filtered by type. |
| GET | /leaderboard | A ranked collection of users including name, points, and current Learning Direction. |

**Response Design Notes**

- Responses use DTOs and contain only data required by the client.
- Authentication credentials and internal Firebase authentication details are not exposed.
- Collection endpoints return collections, while item endpoints return a single resource.
- Create and update operations return the resulting resource when appropriate.
- Successful DELETE operations and bodyless interaction operations return 204 No Content and therefore do not include a response body.
- Interaction results such as counts or current interaction state may be retrieved through relevant GET endpoints or included in content responses when required by the Frontend.

---

### 11.7 Expected Status Codes

The API uses standard HTTP status codes to clearly indicate the result of
each request.

| **Status Code** | **Meaning** | **Usage** |
| --- | --- | --- |
| 200 OK | Request completed successfully. | Successful GET requests and successful PATCH operations that return the updated resource. |
| 201 Created | A new resource was successfully created. | Successful POST requests that create a resource. |
| 204 No Content | Request completed successfully with no response body. | Successful DELETE requests, PUT relationship operations, and bodyless interaction/action operations. |
| 400 Bad Request | The request is invalid. | Invalid request format or invalid input data. |
| 401 Unauthorized | Authentication is required or invalid. | Missing, expired, or invalid Firebase authentication token. |
| 403 Forbidden | The user is authenticated but not allowed to perform the operation. | Accessing or modifying resources that the authenticated user does not have permission to access. |
| 404 Not Found | The requested resource does not exist. | Invalid resource IDs or unavailable resources. |
| 409 Conflict | The request conflicts with the current state of the resource. | Duplicate or conflicting operations such as an existing connection or relationship. |
| 422 Unprocessable Content | The request is syntactically valid but violates applicable domain validation rules. | Domain-specific validation failures when applicable. |

**Endpoint Status Code Summary**

| **Resource / Operation** | **Success Status** |
| --- | --- |
| GET resource / collection | 200 OK |
| POST creating a resource | 201 Created |
| PATCH updating a resource | 200 OK |
| DELETE resource | 204 No Content |
| PUT relationship operation | 204 No Content |
| POST interaction/action without creating a new resource | 204 No Content |

Error responses may additionally return 400, 401, 403, 404, 409, or 422
depending on the reason for failure. The exact error response format will be
standardized during implementation.

---

### 11.8 Authentication Requirements

The platform uses Firebase Authentication to authenticate users. The Backend
verifies the Firebase ID token included with authenticated API requests and
uses the associated firebase_uid to identify the local user.

#### Authentication Rules

| **API Operation** | **Authentication** |
| --- | --- |
| View public profiles | Not required |
| Public retrieval of educational content | Not required |
| Public retrieval of learning directions, skills, and interests | Not required |
| Create or modify a profile | Required |
| Create, modify, or delete educational content | Required |
| Like, save, repost, or share content | Required |
| Create, modify, or delete comments | Required |
| Send or manage connection requests | Required |
| Purchase a connection attempt using points | Required |
| View or manage connections | Required |
| Access conversations and messages | Required |
| Create or manage learning sessions | Required |
| Submit or view ratings | Required |
| Send, view, or respond to skill verification requests | Required |
| Access notifications | Required |
| Access or modify progress | Required |
| Manage experiences | Required |
| Manage personal skills and interests | Required |
| Retrieve personalized recommendations | Required |
| View the leaderboard | Required |
| View own points balance and transaction history | Required |
| Delete the user account | Required |

#### Authentication Flow

```text
User
  ↓
Firebase Authentication
  ↓
Firebase ID Token
  ↓
Backend API
  ↓
Token Verification
  ↓
Identify Local User Using firebase_uid
  ↓
Authorization & Business Rules
  ↓
Process Request
```

#### Authorization

Authentication confirms the user's identity, while authorization determines
whether the authenticated user is allowed to perform the requested operation.

The Backend enforces authorization and ownership rules. For example:

- Users can modify only their own profiles, educational content, comments, experiences, and personal skill/interest relationships.
- Only connected users can access conversations and exchange messages.
- Only eligible participants can create or manage learning sessions.
- Only eligible participants can submit ratings after a qualifying learning session.
- A Skill Verification Request can only be created between a requester and a mentor who share at least one existing Learning Session.
- A newly registered user must complete Onboarding (selecting a Learning Direction, Skills, and Interests) before accessing the Home feed, Recommendations, or other core platform features.
- Users cannot access or modify private data belonging to other users without permission.

#### Firebase Integration

Firebase Authentication manages user authentication credentials. The
application does not store user passwords locally.

The local User entity stores the firebase_uid required to associate the
authenticated Firebase account with the application's user data.

Firebase Authentication is an external authentication service and is
therefore not represented as a database entity in the ERD.

#### Account Deletion

Account deletion is performed through:

`DELETE /users/me`

The current user is identified through Firebase Authentication. The Backend
deletes the user's account and associated data according to the database
deletion rules.

---

### 11.9 References

1. Microsoft Learn. RESTful web API design best practices. Used as a reference for resource-based API design, URI naming, HTTP methods, and RESTful API conventions.
2. Microsoft Learn. Microsoft Graph API design. Used as a reference for designing API resources around client requirements and domain concepts rather than directly exposing the database structure.
3. IETF. RFC 9110: HTTP Semantics. Used as a reference for the semantics of HTTP methods and standard HTTP response status codes.
4. Firebase Documentation. Firebase Authentication. Used as a reference for Firebase Authentication, ID tokens, and backend authentication verification.
````
