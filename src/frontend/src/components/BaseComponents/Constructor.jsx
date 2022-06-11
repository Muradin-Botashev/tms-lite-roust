import React, { useCallback, useMemo } from 'react';
import { clearError } from '../../ducks/gridCard';
import { useDispatch } from 'react-redux';
import { useTranslation } from 'react-i18next';
import { Form, Grid } from 'semantic-ui-react';

const Constructor = ({
    component,
    name: fieldName,
    defaultItems,
    onChange,
    form,
    error,
    columnsConfig,
    settings,
}) => {
    const dispatch = useDispatch();
    const { t } = useTranslation();

    const handleChange = useCallback(
        (e, { name, value, index }) => {
            if (!form[fieldName] || !form[fieldName].length) {
                onChange(e, {
                    name: fieldName,
                    value: [
                        {
                            [name]: value,
                        },
                    ],
                });
            } else {
                const list = [...form[fieldName]];
                list[index] = {
                    ...list[index],
                    [name]: value,
                };
                onChange(e, {
                    name: fieldName,
                    value: [...list],
                });
            }

            if (error.orders) {
                dispatch(clearError && clearError(fieldName));
            }
        },
        [form, error, onChange],
    );

    const handleAdd = useCallback(() => {
        onChange(null, {
            name: fieldName,
            value: [
                ...(form[fieldName] && form[fieldName].length
                    ? form[fieldName]
                    : [
                          {
                              ...defaultItems,
                          },
                      ]),
                {
                    ...defaultItems,
                },
            ],
        });
    }, [form, onChange]);

    const handleDelete = useCallback(
        index => {
            onChange(null, {
                name: fieldName,
                value: form[fieldName].filter((item, itemIndex) => itemIndex !== index),
            });

            Object.keys(defaultItems).forEach(key => {
                if (error[`${key}_${index}`]) {
                    dispatch(clearError && clearError(`${key}_${index}`));
                }
            });
        },
        [form, onChange, error],
    );

    return (
        <Form className="tabs-card constructor-form">
            <Grid>
                {(form[fieldName] && form[fieldName].length
                    ? form[fieldName]
                    : [{ ...defaultItems }]
                ).map((item, index) => (
                    <>
                        {React.cloneElement(component, {
                            item: item,
                            index: index,
                            t: t,
                            defaultOrdersItem: defaultItems,
                            columnsConfig: columnsConfig,
                            settings: settings,
                            error: error,
                            isAdd:
                                (form[fieldName] && index === form[fieldName].length - 1) ||
                                !(form[fieldName] && form[fieldName].length),
                            onChange: handleChange,
                            onDelete: handleDelete,
                            onAdd: handleAdd,
                        })}
                    </>
                ))}
            </Grid>
        </Form>
    );
};

export default Constructor;
