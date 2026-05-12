//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Détail d'un avis en écriture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct AvisClientWrite {
    /// Note sur 5
    pub note: i32,

    /// Commentaire de l'avis
    pub commentaire: Option<String>,

    /// Indique si l'avis est approuvé par le restaurant
    pub approuve: bool,

    /// Client ayant donné l'avis
    pub client_id: i32,

    /// Restaurant concerné par l'avis
    pub restaurant_id: i32,
}
