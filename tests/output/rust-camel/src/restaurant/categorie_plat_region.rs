//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};
use crate::restaurant::categorie_plat::CategoriePlat;

/// Catégories de plats disponibles par région
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct CategoriePlatRegion {
    /// Région
    pub region_code: RegionCode,

    /// Catégorie de plat
    pub categorie_plat: CategoriePlat,
}
