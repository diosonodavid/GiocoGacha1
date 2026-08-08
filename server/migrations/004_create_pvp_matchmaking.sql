CREATE TABLE IF NOT EXISTS pvp_matchmaking (
    user_id UUID PRIMARY KEY REFERENCES users (id) ON DELETE CASCADE,
    mmr INT NOT NULL DEFAULT 1000,
    rank_tier VARCHAR(32) NOT NULL DEFAULT 'Bronze',
    wins INT NOT NULL DEFAULT 0,
    losses INT NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_pvp_matchmaking_mmr ON pvp_matchmaking (mmr);
