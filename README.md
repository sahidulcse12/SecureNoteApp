# SecureNote App

A full-stack secure note-taking application built with **ASP.NET Core 9 (Clean Architecture)** backend and **Next.js** frontend, using **MongoDB** as the database.

---

## 🏗️ Project Structure

```
care guide bd/
├── backend/
│   ├── src/
│   │   ├── Domain/           # Entities, Enums (no dependencies)
│   │   ├── Application/      # Business logic, DTOs, Interfaces, Services
│   │   ├── Infrastructure/   # MongoDB, JWT, Password hashing, Repositories
│   │   └── WebApi/           # Controllers, Middleware, Program.cs
│   └── SecureNoteApp.sln
├── frontend/                 # Next.js App Router + TypeScript + Tailwind CSS
│   └── src/
│       ├── app/              # Pages (login, register, notes, posts, admin/*)
│       ├── components/       # Navbar, RequireAuth
│       ├── context/          # AuthContext
│       └── lib/              # API client (api.ts)
├── mongo_data/               # MongoDB data directory (gitignored)
└── README.md
```

---

## 🚀 Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js 18+
- MongoDB 6+ installed at `C:\Program Files\MongoDB\Server\8.2\bin\mongod.exe`

---

### 1. Start MongoDB

```powershell
& 'C:\Program Files\MongoDB\Server\8.2\bin\mongod.exe' --dbpath 'd:\care guide bd\mongo_data' --port 27017
```

> MongoDB will listen on `localhost:27017`. Leave this terminal open.

---

### 2. Run the Backend

```powershell
cd 'd:\care guide bd'
dotnet run --project backend/src/WebApi/SecureNoteApp.WebApi.csproj
```

The API starts at **http://localhost:5000**

- **Swagger UI:** http://localhost:5000/swagger
- On first run, indexes are created and seed data is loaded automatically.

---

### 3. Run the Frontend

```powershell
cd 'd:\care guide bd\frontend'
npm run dev
```

The frontend starts at **http://localhost:3000**

---

## 🔑 Default Credentials

| Role  | Username | Password   |
|-------|----------|------------|
| Admin | `admin`  | `Admin123!` |
| User  | `alice`  | `User123!`  |
| User  | `bob`    | `User123!`  |
| User  | `charlie`| `User123!`  |

---

## 📡 API Endpoints

### Authentication
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | None | Register new user |
| POST | `/api/auth/login` | None | Login, returns JWT |
| GET | `/api/auth/me` | Bearer | Get current user info |

### Notes
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/notes` | Bearer | Get own notes (paginated) |
| POST | `/api/notes` | Bearer | Create note |
| GET | `/api/notes/{id}` | Bearer | Get note by ID |
| PUT | `/api/notes/{id}` | Bearer | Update note |
| DELETE | `/api/notes/{id}` | Bearer | Delete note |
| GET | `/api/notes/all` | Admin | Get all users' notes |

### Posts
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/posts` | None | Get all posts (paginated) |
| POST | `/api/posts` | Bearer | Create post |

### Users (Admin only)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/users` | Admin | List all users |
| POST | `/api/users` | Admin | Create user |
| GET | `/api/users/{id}` | Admin | Get user |
| PUT | `/api/users/{id}` | Admin | Update user |
| DELETE | `/api/users/{id}` | Admin | Delete user |

### Aggregations
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/aggregations/users-by-interests` | Bearer | Scenario 1: group users by interests |
| GET | `/api/aggregations/users/{userId}/posts` | Bearer | Scenario 2: user + posts via $lookup |
| GET | `/api/aggregations/indexes-info` | Bearer | List all defined indexes |

---

## 🗄️ MongoDB Index Strategy

Indexes are defined using `Schema.Index()` wrappers in `MongoIndexConfigurator.cs`. Only strictly necessary indexes are created — no unnecessary indexes per task constraints.

| Collection | Field(s) | Type | Purpose |
|------------|----------|------|---------|
| users | username | Unique | Login uniqueness |
| users | email | Unique | Email uniqueness |
| users | role | Ascending | Role-based queries |
| users | interests | Ascending | Scenario 1 aggregation ($unwind + $group) |
| notes | userId | Ascending | Fetch user's own notes |
| notes | (userId, createdAt) | Compound | Paginated note queries |
| posts | userId | Ascending | Scenario 2 $lookup join field |
| posts | createdAt | Descending | Sorted public feed |

---

## 🎯 Aggregation Scenarios

### Scenario 1 — Group Users by Interests
Uses **exactly one** `collection.aggregate()` call with `$unwind` + `$group`:
```
GET /api/aggregations/users-by-interests
```
Pipeline: `$unwind: "$interests"` → `$group: { _id: "$interests", count, users }` → `$sort`

### Scenario 2 — User Posts via `$lookup`
Uses a **single aggregation pipeline** with a `$lookup` stage:
```
GET /api/aggregations/users/{userId}/posts
```
Pipeline: `$match` → `$lookup: { from: "posts", localField: "_id", foreignField: "userId" }` → `$project`

---

## 🛡️ Role-Based Access Control

| Feature | Regular User | Admin |
|---------|-------------|-------|
| Own notes (CRUD) | ✅ | ✅ |
| All notes (view/delete) | ❌ | ✅ |
| Create posts | ✅ | ✅ |
| View posts | ✅ | ✅ |
| User management | ❌ | ✅ |
| Aggregation endpoints | ✅ | ✅ |

---

## 🔧 Architecture

```
Request → Controller → Service (Application) → Repository (Infrastructure) → MongoDB
                   ↑                        ↑
              ICurrentUserService      IJwtTokenGenerator / IPasswordHasher
```

- **Domain:** Pure C# entities, no framework dependencies
- **Application:** Business rules, interfaces, DTOs — references only Domain
- **Infrastructure:** MongoDB Driver, BCrypt, JWT implementation
- **WebApi:** ASP.NET Core host, controllers, middleware, DI composition root
