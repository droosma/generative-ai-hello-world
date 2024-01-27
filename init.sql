CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE IF NOT EXISTS embeddings (
  id SERIAL PRIMARY KEY,
  embedding vector(1536),
  content text,
  reference text,
  created_at timestamptz DEFAULT now()
);

CREATE INDEX ON embeddings USING ivfflat (embedding vector_l2_ops) WITH (lists = 100);