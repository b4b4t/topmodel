//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Détail d'un plat en écriture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct PlatWrite {
    /// Nom du plat
    pub nom: String,

    /// Description du plat
    pub description: Option<String>,

    /// Prix du plat
    pub prix: Decimal,

    /// Indique si le plat est disponible
    pub disponible: bool,

    /// Catégorie du plat
    pub categorie_plat_code: CategoriePlatCode,

    /// Restaurant proposant ce plat
    pub restaurant_id: i32,
}
