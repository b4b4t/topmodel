//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};
use crate::restaurant::menu_plat::MenuPlat;
use crate::restaurant::restaurant::Restaurant;

/// Menu du restaurant
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Menu {
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
    pub restaurant: Restaurant,

    /// Association réciproque de MenuPlat.Menu
    pub plats: Vec<MenuPlat>,
}
