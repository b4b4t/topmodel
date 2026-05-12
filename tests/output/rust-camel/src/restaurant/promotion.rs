//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use serde::{Deserialize, Serialize};
use crate::restaurant::plat::Plat;
use crate::restaurant::restaurant::Restaurant;

/// Promotion sur un plat
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Promotion {
    /// Plat concerné par la promotion.
    pub plat: Plat,

    /// Libellé de la promotion
    pub libelle: String,

    /// Pourcentage de réduction (0-100)
    pub pourcentage_reduction: i32,

    /// Date de début de la promotion
    pub date_debut: NaiveDateTime,

    /// Date de fin de la promotion
    pub date_fin: NaiveDateTime,

    /// Indique si la promotion est active
    pub active: bool,

    /// Restaurant concerné par la promotion (null si globale)
    pub restaurant: Option<Restaurant>,
}
