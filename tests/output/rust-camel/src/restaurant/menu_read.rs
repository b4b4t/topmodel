//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::categorie_plat::CategoriePlat;
use crate::restaurant::plat_item::PlatItem;

/// Détail d'un menu en lecture
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct MenuRead {
    /// Identifiant du menu
    pub id: i32,

    /// Nom du menu
    pub nom: String,

    /// Description du menu
    pub description: Option<String>,

    /// Prix du menu
    pub prix: Decimal,

    /// Indique si le menu est disponible
    pub disponible: bool,

    /// Date de début de validité du menu
    pub date_debut: Option<NaiveDateTime>,

    /// Date de fin de validité du menu
    pub date_fin: Option<NaiveDateTime>,

    /// Restaurant proposant ce menu
    pub restaurant_id: i32,

    /// Catégories de plat dans le menu.
    pub categories_plat: Vec<CategoriePlat>,

    /// Liste des plats du menu
    pub plats: Vec<PlatItem>,
}
