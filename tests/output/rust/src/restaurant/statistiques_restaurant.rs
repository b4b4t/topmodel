//!
//! ATTENTION CE FICHIER EST GENERE AUTOMATIQUEMENT !
//!

use rust_decimal::Decimal;
use serde::{Deserialize, Serialize};

/// Statistiques d'un restaurant
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StatistiquesRestaurant {
    /// Identifiant du restaurant
    pub restaurant_id: i32,

    /// Nombre total de commandes
    pub nombre_commandes: i32,

    /// Chiffre d'affaires total
    pub chiffre_affaires: Decimal,

    /// Nombre de clients uniques
    pub nombre_clients: i32,

    /// Note moyenne des avis
    pub note_moyenne: Option<Decimal>,
}
