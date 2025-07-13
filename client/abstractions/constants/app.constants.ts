export const DEFAULT_CATEGORIES = {
  EXPENSE: [
    { name: 'Food & Dining', color: '#FF6B6B', icon: 'restaurant' },
    { name: 'Transportation', color: '#4ECDC4', icon: 'directions_car' },
    { name: 'Shopping', color: '#45B7D1', icon: 'shopping_cart' },
    { name: 'Entertainment', color: '#96CEB4', icon: 'movie' },
    { name: 'Bills & Utilities', color: '#FFEAA7', icon: 'receipt' },
    { name: 'Healthcare', color: '#DDA0DD', icon: 'local_hospital' },
    { name: 'Education', color: '#98D8C8', icon: 'school' },
    { name: 'Travel', color: '#F7DC6F', icon: 'flight' },
    { name: 'Rent', color: '#CD853F', icon: 'home' },
    { name: 'Other', color: '#BDC3C7', icon: 'more_horiz' }
  ],
  INCOME: [
    { name: 'Salary', color: '#27AE60', icon: 'work' },
    { name: 'Freelance', color: '#2ECC71', icon: 'computer' },
    { name: 'Investment', color: '#16A085', icon: 'trending_up' },
    { name: 'Gift', color: '#F39C12', icon: 'card_giftcard' },
    { name: 'Other Income', color: '#BDC3C7', icon: 'more_horiz' }
  ]
};

export const CURRENCY_SYMBOLS = {
  USD: '$',
  EUR: '€',
  GBP: '£',
  JPY: '¥',
  CAD: 'C$',
  AUD: 'A$'
};

export const DATE_FORMATS = {
  DISPLAY: 'MMM dd, yyyy',
  INPUT: 'yyyy-MM-dd',
  FULL: 'MMMM dd, yyyy HH:mm'
};

export const PAGINATION = {
  DEFAULT_PAGE_SIZE: 20,
  MAX_PAGE_SIZE: 100
};

export const VALIDATION = {
  MAX_DESCRIPTION_LENGTH: 255,
  MAX_NOTES_LENGTH: 1000,
  MIN_AMOUNT: 0.01,
  MAX_AMOUNT: 999999.99
};
