//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Statut d'une commande
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum StatutCommande {
    /// Annulée
    Annulee,

    /// En attente
    EnAttente,

    /// En préparation
    EnPreparation,

    /// Prête
    Prete,

    /// Servie
    Servie,
}
