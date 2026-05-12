//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Ligne de commande pour historique avec préservation des clés primaires
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LigneCommandeHistorique {
    /// Identifiant de la ligne
    pub id: i32,

    /// Quantité commandée
    pub quantite: i32,

    /// Prix unitaire au moment de la commande
    pub prix_unitaire: Decimal,

    /// Prix total de la ligne
    pub prix_total: Decimal,

    /// Plat commandé
    pub plat_id: i32,

    /// Commande à laquelle appartient la ligne
    pub commande_historique_id: i32,
}
