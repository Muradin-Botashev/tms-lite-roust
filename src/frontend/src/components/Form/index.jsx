import React from 'react';
import { ROW_TAB_TYPE } from '../../constants/formTypes';
import RowForm from './Row';

const getTypeForm = {
    [ROW_TAB_TYPE]: <RowForm />,
};

const FormCard = ({ type }) => {
    return <h1>Form</h1>;
};

export default FormCard;
