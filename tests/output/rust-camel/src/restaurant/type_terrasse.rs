//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};

/// Type de terrasse
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize, sqlx::Type)]
#[serde(rename_all = "SCREAMING_SNAKE_CASE")]
#[sqlx(type_name = "type_terrasse", rename_all = "lowercase")]
pub enum TypeTerrasse {
    /// EXT
    Ext,

    /// INT
    Int,
}
