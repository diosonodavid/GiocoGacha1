const K_FACTOR = 32;

export interface EloResult {
  newAttackerMmr: number;
  newDefenderMmr: number;
}

// Standard Elo: expected score is a logistic curve over the rating gap, and each side moves
// toward the actual outcome by K * (actual - expected). The two deltas are equal and opposite
// only when both sides use the same K, which they do here.
function expectedScore(ratingA: number, ratingB: number): number {
  return 1 / (1 + Math.pow(10, (ratingB - ratingA) / 400));
}

export function calculateEloChange(attackerMmr: number, defenderMmr: number, attackerWon: boolean): EloResult {
  const expectedAttacker = expectedScore(attackerMmr, defenderMmr);
  const expectedDefender = 1 - expectedAttacker;

  const actualAttacker = attackerWon ? 1 : 0;
  const actualDefender = 1 - actualAttacker;

  const newAttackerMmr = Math.round(attackerMmr + K_FACTOR * (actualAttacker - expectedAttacker));
  const newDefenderMmr = Math.round(defenderMmr + K_FACTOR * (actualDefender - expectedDefender));

  return { newAttackerMmr, newDefenderMmr };
}

export function getRankTierForMmr(mmr: number): string {
  if (mmr >= 2200) return 'Grandmaster';
  if (mmr >= 1900) return 'Master';
  if (mmr >= 1600) return 'Diamond';
  if (mmr >= 1300) return 'Platinum';
  if (mmr >= 1000) return 'Gold';
  if (mmr >= 700) return 'Silver';
  return 'Bronze';
}
