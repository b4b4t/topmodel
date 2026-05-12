//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use serde::{Deserialize, Serialize};
use crate::restaurant::client::Client;
use crate::restaurant::restaurant::Restaurant;

/// Réservation d'une table
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Reservation {
    /// Identifiant de la réservation
    pub id: i32,

    /// Date et heure de la réservation
    pub date_reservation: NaiveDateTime,

    /// Nombre de personnes
    pub nombre_personnes: i32,

    /// Commentaire sur la réservation
    pub commentaire: Option<String>,

    /// Indique si la réservation est confirmée
    pub confirmee: bool,

    /// Client ayant fait la réservation
    pub client: Client,

    /// Table réservée
    pub table_id: Option<i32>,

    /// Restaurant concerné par la réservation
    pub restaurant: Restaurant,
}
