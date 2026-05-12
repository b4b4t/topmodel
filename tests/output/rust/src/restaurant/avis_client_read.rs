//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use serde::{Deserialize, Serialize};

/// Détail d'un avis en lecture
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AvisClientRead {
    /// Identifiant de l'avis
    pub id: i32,

    /// Note sur 5
    pub note: i32,

    /// Commentaire de l'avis
    pub commentaire: Option<String>,

    /// Date de l'avis
    pub date_avis: NaiveDateTime,

    /// Indique si l'avis est approuvé par le restaurant
    pub approuve: bool,

    /// Nombre de vues de l'avis (calculé)
    pub nombre_vues: i32,

    /// Client ayant donné l'avis
    pub client_id: i32,

    /// Restaurant concerné par l'avis
    pub restaurant_id: i32,
}
