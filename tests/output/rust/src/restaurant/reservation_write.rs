//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use serde::{Deserialize, Serialize};

/// Détail d'une réservation en écriture
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ReservationWrite {
    /// Date et heure de la réservation
    pub date_reservation: NaiveDateTime,

    /// Nombre de personnes
    pub nombre_personnes: i32,

    /// Commentaire sur la réservation
    pub commentaire: Option<String>,

    /// Indique si la réservation est confirmée
    pub confirmee: bool,

    /// Client ayant fait la réservation
    pub client_id: i32,

    /// Table réservée
    pub table_id: Option<i32>,

    /// Restaurant concerné par la réservation
    pub restaurant_id: i32,
}
