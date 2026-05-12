//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use serde::{Deserialize, Serialize};
use crate::restaurant::client::Client;
use crate::restaurant::restaurant::Restaurant;

/// Avis d'un client sur un restaurant
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct AvisClient {
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
    pub client: Client,

    /// Restaurant concerné par l'avis
    pub restaurant: Restaurant,
}
