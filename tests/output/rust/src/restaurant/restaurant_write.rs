//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use serde::{Deserialize, Serialize};
use crate::restaurant::table_item::TableItem;

/// Détail d'un restaurant en écriture
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RestaurantWrite {
    /// Nom du restaurant
    pub nom: String,

    /// Adresse du restaurant
    pub adresse: Option<String>,

    /// Numéro de téléphone
    pub telephone: Option<String>,

    /// Association réciproque de Menu.Restaurant
    pub menus: Vec<i32>,

    /// Association réciproque de Plat.Restaurant
    pub plats: Vec<i32>,

    /// Association réciproque de Promotion.Restaurant
    pub promotions: Option<Vec<i32>>,

    /// Association réciproque de AvisClient.Restaurant
    pub avis_clients: Vec<i32>,

    /// Association réciproque de TableRestaurant.RestaurantId
    pub tables: Vec<TableItem>,
}
