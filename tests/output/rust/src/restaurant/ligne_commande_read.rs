//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Détail d'une ligne de commande en lecture
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LigneCommandeRead {
    /// Identifiant de la ligne
    pub id: i32,

    /// Quantité commandée
    pub quantite: i32,

    /// Prix unitaire au moment de la commande
    pub prix_unitaire: Decimal,

    /// Prix total de la ligne
    pub prix_total: Decimal,

    /// Commande à laquelle appartient la ligne
    pub commande_id: i32,

    /// Plat commandé
    pub plat_id: i32,
}
