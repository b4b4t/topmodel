//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Statut d'une commande
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize, sqlx::Type)]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
#[sqlx(type_name = "statut_commande", rename_all = "lowercase")]
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
