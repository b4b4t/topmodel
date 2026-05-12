//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::client_write::ClientWrite;
use crate::restaurant::ligne_commande_write::LigneCommandeWrite;
use crate::restaurant::reservation_write::ReservationWrite;
use crate::restaurant::statut_commande::StatutCommande;

/// Détail d'une commande en écriture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct CommandeWrite {
    /// Date et heure de la commande
    pub date_commande: NaiveDateTime,

    /// Date et heure de livraison
    pub date_livraison: Option<NaiveDateTime>,

    /// Montant total de la commande
    pub montant_total: Decimal,

    /// Table associée à la commande
    pub table_id: Option<i32>,

    /// Statut de la commande
    pub statut_commande: StatutCommande,

    /// Avis laissé par le client sur la commande.
    pub avis_client_id: Option<i32>,

    /// Client ayant passé la commande
    pub client: ClientWrite,

    /// Réservation.
    pub reservation: Option<ReservationWrite>,

    /// Association réciproque de LigneCommande.Commande
    pub lignes: Vec<LigneCommandeWrite>,
}
