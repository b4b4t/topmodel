//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};
use crate::restaurant::menu::Menu;
use crate::restaurant::plat::Plat;

/// Plat dans un menu
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct MenuPlat {
    /// Menu contenant ce plat
    pub menu: Menu,

    /// Plat du menu
    pub plat: Plat,

    /// Ordre d'affichage du plat dans le menu
    pub ordre: i32,
}
