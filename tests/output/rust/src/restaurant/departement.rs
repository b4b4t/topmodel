//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Département
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Departement {
    /// Code du département.
    pub code: String,

    /// Libellé du département.
    pub libelle: String,

    /// Région associée.
    pub region_code: RegionCode,
}
