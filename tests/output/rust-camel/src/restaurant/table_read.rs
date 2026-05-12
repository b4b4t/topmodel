//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Détail d'une table en lecture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct TableRead {
    /// Identifiant de la table
    pub id: i32,

    /// Numéro de la table
    pub numero: String,

    /// Capacité de la table (nombre de places)
    pub capacite: i32,

    /// Indique si la table est disponible
    pub disponible: bool,

    /// Restaurant auquel appartient la table
    pub restaurant_id: i32,
}
