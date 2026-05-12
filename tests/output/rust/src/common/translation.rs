//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Table pour stocker les traductions en SQL.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Translation {
    /// Clé de traduction.
    pub resource_key: String,

    /// Valeur de la clé de traduction.
    pub value: String,
}
