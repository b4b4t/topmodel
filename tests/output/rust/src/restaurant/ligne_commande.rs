//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::commande::Commande;
use crate::restaurant::plat::Plat;

/// Ligne d'une commande
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LigneCommande {
    /// Identifiant de la ligne
    pub id: i32,

    /// Quantité commandée
    pub quantite: i32,

    /// Prix unitaire au moment de la commande
    pub prix_unitaire: Decimal,

    /// Prix total de la ligne
    pub prix_total: Decimal,

    /// Commande à laquelle appartient la ligne
    pub commande: Commande,

    /// Plat commandé
    pub plat: Plat,
}
