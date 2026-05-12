//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::avis_client::AvisClient;
use crate::restaurant::client::Client;
use crate::restaurant::ligne_commande::LigneCommande;
use crate::restaurant::reservation::Reservation;
use crate::restaurant::statut_commande::StatutCommande;

/// Commande d'un client
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Commande {
    /// Identifiant de la commande
    pub id: i32,

    /// Date et heure de la commande
    pub date_commande: NaiveDateTime,

    /// Date et heure de livraison
    pub date_livraison: Option<NaiveDateTime>,

    /// Montant total de la commande
    pub montant_total: Decimal,

    /// Client ayant passé la commande
    pub client: Client,

    /// Table associée à la commande
    pub table_id: Option<i32>,

    /// Réservation associée à la commande
    pub reservation: Option<Reservation>,

    /// Statut de la commande
    pub statut_commande: StatutCommande,

    /// Avis laissé par le client sur la commande.
    pub avis_client: Option<AvisClient>,

    /// Association réciproque de LigneCommande.Commande
    pub lignes: Vec<LigneCommande>,
}
