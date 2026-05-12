//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::statut_commande::StatutCommande;

/// Commande pour historique avec préservation des clés primaires
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct CommandeHistorique {
    /// Identifiant de la commande
    pub id: i32,

    /// Date et heure de la commande
    pub date_commande: NaiveDateTime,

    /// Date et heure de livraison
    pub date_livraison: Option<NaiveDateTime>,

    /// Montant total de la commande
    pub montant_total: Decimal,

    /// Client ayant passé la commande
    pub client_id: i32,

    /// Table associée à la commande
    pub table_id: Option<i32>,

    /// Réservation associée à la commande
    pub reservation_id: Option<i32>,

    /// Statut de la commande
    pub statut_commande: StatutCommande,

    /// Avis laissé par le client sur la commande.
    pub avis_client_id: Option<i32>,
}
