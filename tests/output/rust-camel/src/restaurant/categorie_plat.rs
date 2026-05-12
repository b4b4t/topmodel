//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Catégorie de plat
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct CategoriePlat {
    /// Code de la catégorie
    pub code: CategoriePlatCode,

    /// Libellé de la catégorie
    pub libelle: String,

    /// Ordre d'affichage dans le menu.
    pub ordre: i32,

    /// Prix moyen de la catégorie, à titre indicatif.
    pub prix_moyen: Option<Decimal>,
}
