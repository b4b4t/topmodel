//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Table du restaurant
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct TableRestaurant {
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
