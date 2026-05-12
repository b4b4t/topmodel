//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use chrono::NaiveDateTime;
use serde::{Deserialize, Serialize};

/// Détail d'une promotion en écriture
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PromotionWrite {
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
    pub restaurant_id: Option<i32>,
}
