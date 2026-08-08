CREATE TABLE IF NOT EXISTS pvp_defense_teams (
    user_id UUID PRIMARY KEY REFERENCES users (id) ON DELETE CASCADE,
    leader_id VARCHAR(64) NOT NULL,
    team_json JSONB NOT NULL,
    total_power INT NOT NULL DEFAULT 0,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
