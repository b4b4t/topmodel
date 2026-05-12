//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Détail d'un menu en écriture
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct MenuWrite {
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

    /// Catégories de plat disponibles dans le menu.
    pub categories_plat: Vec<CategoriePlatCode>,
}
