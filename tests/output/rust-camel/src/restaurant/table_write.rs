//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Détail d'une table en écriture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct TableWrite {
    /// Numéro de la table
    pub numero: String,

    /// Capacité de la table (nombre de places)
    pub capacite: i32,

    /// Indique si la table est disponible
    pub disponible: bool,

    /// Restaurant auquel appartient la table
    pub restaurant_id: i32,
}
