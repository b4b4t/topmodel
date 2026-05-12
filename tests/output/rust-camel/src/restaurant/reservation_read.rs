//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use serde::{Deserialize, Serialize};

/// Détail d'une réservation en lecture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct ReservationRead {
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
    pub client_id: i32,

    /// Table réservée
    pub table_id: Option<i32>,

    /// Restaurant concerné par la réservation
    pub restaurant_id: i32,

    /// Informations du client ayant fait la réservation
    pub client_nom: String,

    /// Informations du client ayant fait la réservation
    pub client_prenom: String,

    /// Informations du client ayant fait la réservation
    pub client_email: Option<String>,

    /// Table réservée
    pub table_numero: String,

    /// Table réservée
    pub table_capacite: i32,
}
