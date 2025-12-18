-- Base version of the database ai generated and adjusted. 
-- Todo: Adjust if necessary.


CREATE TABLE IF NOT EXISTS users (
  id INT AUTO_INCREMENT PRIMARY KEY,
  username VARCHAR(100) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  first_name VARCHAR(100) NOT NULL,
  last_name  VARCHAR(100) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_users_username (username)
);

CREATE TABLE IF NOT EXISTS projects (
  id INT AUTO_INCREMENT PRIMARY KEY,
  user_id INT NOT NULL,
  name VARCHAR(255) NOT NULL,
  topic VARCHAR(255) NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_projects_user FOREIGN KEY (user_id)
    REFERENCES users(id) ON DELETE CASCADE,
  INDEX idx_projects_user (user_id)
);

CREATE TABLE IF NOT EXISTS criteria_progress (
  id INT AUTO_INCREMENT PRIMARY KEY,
  project_id INT NOT NULL,
  criteria_id VARCHAR(50) NOT NULL,
  fulfilled_requirement_ids JSON NOT NULL DEFAULT ('[]'),
  notes NVARCHAR(1000) NULL,
  last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  CONSTRAINT fk_progress_project FOREIGN KEY (project_id)
    REFERENCES projects(id) ON DELETE CASCADE,
  UNIQUE KEY uq_project_criteria (project_id, criteria_id),
  INDEX idx_progress_project (project_id),
  INDEX idx_progress_criteria (criteria_id)
);
