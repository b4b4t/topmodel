//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Département
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize, sqlx::FromRow)]
#[serde(rename_all = "camelCase")]
pub struct Departement {
    /// Code du département.
    pub code: String,

    /// Libellé du département.
    pub libelle: String,

    /// Région associée.
    pub region_code: RegionCode,
}
