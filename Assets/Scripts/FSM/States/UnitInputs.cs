public enum UnitInputs
{
    // Sensores y Entorno
    None,
    EnemySpotted,    // Vi a un enemigo
    LostSight,       // Perdí de vista al enemigo
    LowHealth,       // Vida baja (Trigger de supervivencia)
    Safe,            // Ya no hay peligro
    ArrivedAtPoint,  // Llegué al destino
    
    // Decisiones Tácticas (Roulette / Líder)
    DecisionAttack,  // La ruleta decidió atacar
    DecisionRetreat, // La ruleta decidió huir
    DecisionDefend   // La ruleta decidió cubrirse
}
