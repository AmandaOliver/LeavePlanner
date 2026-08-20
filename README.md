# LeavePlanner

Leave and absence planning for small organisations. React + TypeScript frontend, ASP.NET Core 8 API, MySQL, with authentication delegated to Auth0.

---

## Configuration

The API stores **no user passwords**. Authentication is delegated to Auth0 — the API only ever validates a bearer token, and the `Employees` table has no password column. The credentials below are the ones the service needs to reach its own dependencies.

Nothing secret is committed. ASP.NET Core reads configuration from `appsettings.json`, then environment variables, then user-secrets in development, with later sources overriding earlier ones — so secrets are supplied out-of-band and the committed files hold structure only.

### What the system needs

| Setting | Secret | Local | Deployed |
| --- | --- | --- | --- |
| `ConnectionStrings:LeavePlannerDB` | **yes** | user-secrets | `ConnectionStrings__LeavePlannerDB` |
| `Email:Password` | **yes** | user-secrets | `Email__Password` |
| `Email:FromAddress` | no | `appsettings.Development.json` | `appsettings.json` |
| `App:FrontendUrl` | no | `appsettings.Development.json` | `appsettings.json` |
| `Auth0:Domain` / `Auth0:Audience` | no | `appsettings.Development.json` | `appsettings.json` |
| `REACT_APP_AUTH0_*` | no | `frontend/.env.development` | build environment |
| `E2E_USER` / `E2E_PASSWORD` | **password only** | `frontend/.env.local` | CI repository secrets |

`API/appsettings.Example.json` and `frontend/.env.example` are fill-in-the-blanks templates for the non-secret keys. Neither is loaded at runtime.

Two of these are deliberately *not* treated as secrets. The Auth0 domain and the SPA client ID are public identifiers that ship in the browser bundle and travel in every token; they live in configuration because they are environment-specific, not because they are confidential.

Any missing or malformed value **fails the boot** with a message naming the key, rather than surfacing as a confusing error at request time.

---

## First-time setup

### 1. Auth0

Create two tenants — one for development and e2e tests, one for production — so test users never share a directory with real ones.

In each tenant:

1. **Applications → Create Application** → *Single Page Application*. Note the **Client ID** (public).
   - *Allowed Callback URLs*: `https://localhost:3000/home`
   - *Allowed Logout URLs* and *Allowed Web Origins*: `https://localhost:3000`
2. **APIs → Create API**. Set the *Identifier* to `https://api.leaveplanner.org` — this is the `Audience`.
3. Add the user's email to the access token, otherwise every request is rejected as unauthenticated. Under **Actions → Library → Build Custom**, add a Login / Post Login action and deploy it:

   ```js
   exports.onExecutePostLogin = async (event, api) => {
     api.accessToken.setCustomClaim('email', event.user.email)
   }
   ```

   The API resolves the caller by the `email` claim (`NameClaimType = "email"` in `Program.cs`), so this action is required, not optional.
4. For the test tenant, enable a **Username-Password-Authentication** database connection and create one test user. The e2e suite signs in against this — it must not drive a real Google account.

### 2. Database

```bash
mysql -u root -p < DB/database_schema.sql
```

Then create a least-privilege application user. The API should never connect as `root` or as the RDS master account:

```sql
CREATE USER 'leaveplanner_app'@'%' IDENTIFIED BY 'a-strong-generated-password';
GRANT SELECT, INSERT, UPDATE, DELETE ON LeavePlanner.* TO 'leaveplanner_app'@'%';
FLUSH PRIVILEGES;
```

### 3. API

Fill in the non-secret values in `API/appsettings.Development.json` (`Auth0:Domain`, `Auth0:Audience`), then store the secrets outside the repository:

```bash
cd API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:LeavePlannerDB" \
  "server=127.0.0.1;port=3306;user=leaveplanner_app;password=...;database=LeavePlanner"

dotnet run
```

User-secrets are written to `~/.microsoft/usersecrets/`, outside the working tree.

Email is off by default in development (`Email:Enabled: false`); the service logs what it would have sent. To exercise real delivery, set `Email:Enabled` to `true`, set `Email:FromAddress`, and add the password:

```bash
dotnet user-secrets set "Email:Password" "your-smtp-app-password"
```

For Gmail this is an **App Password** (Google Account → Security → 2-Step Verification → App passwords), not the account password.

### 4. Frontend

```bash
cd frontend
yarn install
cp .env.example .env.development   # fill in REACT_APP_AUTH0_*
yarn setup:certs                   # generates localhost.key / localhost.crt
yarn start
```

The dev server runs over HTTPS because Auth0 requires it for the callback. The certificate is self-signed and generated locally — it is gitignored, and your browser will ask you to trust it once.

### 5. Deployment

Set the secrets as environment variables on the host, replacing `:` with `__`:

```bash
ConnectionStrings__LeavePlannerDB="server=...;user=leaveplanner_app;password=...;database=LeavePlanner"
Email__Password="..."
```

Everything else comes from `appsettings.json`.

---

## Running the e2e tests

Playwright signs in once in `globalSetup.ts` and caches the session in `auth.json` (gitignored — it holds a live token), which every spec reuses.

```bash
cd frontend
cp .env.example .env.local   # fill in E2E_USER and E2E_PASSWORD
yarn test
```

`.env.local` is gitignored. In CI, supply `E2E_USER` and `E2E_PASSWORD` as repository secrets instead. The suite fails fast with a pointer to this section if either is missing.

Both the API and the frontend need to be running first.

---

## Secret handling

CI scans the full history with [gitleaks](.github/workflows/secret-scan.yml) on every push. Run the same check before committing:

```bash
gitleaks protect --staged
```

Enabling GitHub's **push protection** (Settings → Code security) adds a server-side block and is free on public repositories.

If a credential is ever exposed, rotate it before anything else: a published secret stays compromised no matter what the history says afterwards.

### Why not a secrets manager?

Environment variables and user-secrets are the right size for a project this size: no extra infrastructure, no new failure mode, and both are built into the platform. AWS Secrets Manager or Vault start earning their keep when there are enough engineers that credential distribution becomes a process problem, or when an audit requires automatic rotation on a schedule. Neither is true here yet — so this is a deliberate stopping point, not an omission.
