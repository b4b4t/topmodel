//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::categorie_plat::CategoriePlat;
use crate::restaurant::promotion::Promotion;
use crate::restaurant::restaurant::Restaurant;

/// Plat du menu
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Plat {
    /// Identifiant du plat
    pub id: i32,

    /// Nom du plat
    pub nom: String,

    /// Description du plat
    pub description: Option<String>,

    /// Prix du plat
    pub prix: Decimal,

    /// Indique si le plat est disponible
    pub disponible: bool,

    /// Catégorie du plat
    pub categorie_plat: CategoriePlat,

    /// Restaurant proposant ce plat
    pub restaurant: Restaurant,

    /// Association réciproque de Promotion.Plat
    pub promotion: Option<Promotion>,
}
