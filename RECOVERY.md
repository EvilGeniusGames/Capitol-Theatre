# 📦 Capitol Theatre — Installation & Recovery Manual

## 🛠 First-Time Installation (Docker)

1. **Clone the Repository**
   ```bash
   git clone https://github.com/EvilGeniusGames/Capitol-Theatre.git
   cd Capitol-Theatre
   ```

2. **Edit Environment Settings**
   Create or modify the `.env` file in the project root to set the initial admin credentials:
   ```env
   AdminEmail=admin@example.com
   AdminPassword=YourSecurePassword123
   ```

3. **Launch the App**
   This creates the required volume structure and seeds the admin user:
   ```bash
   docker compose up -d
   ```

4. **Stop the App (Optional, for Restore)**
   If restoring from a backup:
   ```bash
   docker compose down
   ```

---

## ♻️ Recovery from Backup

> Use this if you're restoring a previously downloaded backup ZIP.

1. **Unzip the Backup**
   Extract the contents of `backup-YYYYMMDD-HHmmss.zip` into the `SiteData/` volume directory used by Docker. This includes:
   - `scms.db` → `/app/data/scms.db`
   - `images/` → `/app/wwwroot/images/`
   - `README.txt` (optional)

2. **Ensure File Placement**
   After extraction, your directory layout inside the container should be:
   ```
   /app/data/scms.db
   /app/wwwroot/images/...
   ```

3. **Restart the App**
   ```bash
   docker compose up -d
   ```

---

## 🔐 Notes

- The `.env` file is required only for the **first** startup to seed the admin user.
- For security, ensure the `.env` file is not committed to version control.
- You may delete the extracted backup zip once restore is complete.