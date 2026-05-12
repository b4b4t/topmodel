//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Région
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Region {
    /// Code de la région.
    pub code: RegionCode,

    /// Libellé de la région.
    pub libelle: String,

    /// Nom du responsable de la région.
    pub nom_responsable: Option<String>,
}
